using Hangfire;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using RealSync.Api.Extensions;
using RealSync.Api.Middlewares;
using RealSync.Data.Context;
using RealSync.Data.Seeders;
using Serilog;

// ===== Serilog Bootstrap =====
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ===== Serilog =====
    builder.Host.UseSerilog((context, config) => config
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/realsync-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30));

    // ===== Services =====

    // DbContext
    builder.Services.AddDbContext<RealSyncDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("RealSync.Data");
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));



    // Application services (DI)
    builder.Services.AddApplicationServices();

    // Hangfire background processing
    builder.Services.AddHangfireServices(builder.Configuration);

    // Storage Options
    builder.Services.AddOptions<RealSync.Services.Options.StorageOptions>()
        .Bind(builder.Configuration.GetSection(RealSync.Services.Options.StorageOptions.SectionName))
        .ValidateDataAnnotations()
        .Validate(options => !string.IsNullOrWhiteSpace(options.RootPath), "Storage:RootPath is required.")
        .ValidateOnStart();

    builder.Services.Configure<RealSync.Services.Options.FollowUpReminderOptions>(
        builder.Configuration.GetSection("FollowUpReminders"));

    // JWT Authentication
    builder.Services.AddJwtAuthentication(builder.Configuration);

    // Vault Options
    builder.Services.Configure<RealSync.Core.Models.Publishing.VaultOptions>(
        builder.Configuration.GetSection(RealSync.Core.Models.Publishing.VaultOptions.SectionName));

    // CORS
    builder.Services.AddCorsPolicy(builder.Configuration);

    // AI Options (Gemini / OpenAI) — config trong appsettings.json
    builder.Services.Configure<RealSync.Services.Options.AIOptions>(
        builder.Configuration.GetSection("AI"));

    // Video Options (Veo / FFmpeg)
    builder.Services.Configure<RealSync.Services.Options.VideoOptions>(
        builder.Configuration.GetSection("Video"));

    // TikTok Options
    builder.Services.Configure<RealSync.Services.Options.TikTokOptions>(
        builder.Configuration.GetSection(RealSync.Services.Options.TikTokOptions.SectionName));

    // Controllers with FluentValidation filter
    builder.Services.AddControllersWithValidation();

    // Swagger with JWT
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerWithJwt();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<RealSyncDbContext>("database");

    // Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("GlobalPolicy", opt =>
        {
            opt.PermitLimit = 100;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 2;
        });
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    var app = builder.Build();

    // ===== Storage Validation & Initialization =====
    {
        var storageOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RealSync.Services.Options.StorageOptions>>().Value;
        var absoluteRoot = Path.GetFullPath(storageOptions.RootPath);
        var driveRoot = Path.GetPathRoot(absoluteRoot);

        if (!string.IsNullOrEmpty(driveRoot) && !Directory.Exists(driveRoot))
        {
            Log.Fatal("Γ¥î Storage drive {DriveRoot} not found or not connected. Path: {RootPath}", driveRoot, absoluteRoot);
            throw new Exception($"ß╗ö ─æ─⌐a l╞░u trß╗» '{driveRoot}' kh├┤ng tß╗ôn tß║íi hoß║╖c ch╞░a ─æ╞░ß╗úc kß║┐t nß╗æi. Vui l├▓ng kiß╗âm tra lß║íi cß║Ñu h├¼nh Storage.");
        }

        try
        {
            Directory.CreateDirectory(absoluteRoot);
            Directory.CreateDirectory(Path.GetFullPath(storageOptions.PublicPath));
            Directory.CreateDirectory(Path.GetFullPath(storageOptions.PrivatePath));
            Directory.CreateDirectory(Path.GetFullPath(storageOptions.TempPath));
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Γ¥î Failed to create required storage directories under {RootPath}", absoluteRoot);
            throw;
        }
    }

    // ===== Database Migration & Seed (Development only) =====
    if (app.Environment.IsDevelopment() &&
        app.Configuration.GetValue("Database:AutoMigrate", true))
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RealSyncDbContext>();
        await dbContext.Database.MigrateAsync();
        await DataSeeder.SeedAsync(dbContext);
    }

    // ===== Middleware Pipeline =====
    app.UseSerilogRequestLogging();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseRateLimiter();
    app.UseStaticFiles();

    // Map public storage static files
    {
        var storageOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RealSync.Services.Options.StorageOptions>>().Value;
        var absolutePublicPath = Path.GetFullPath(storageOptions.PublicPath);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(absolutePublicPath),
            RequestPath = "/uploads"
        });
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "RealSync API v1");
            options.DocumentTitle = "RealSync API";
        });
    }

    app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<ApiActivityAuditMiddleware>();

    // Hangfire Dashboard is development-only. Do not expose it on staging/production.
    if (app.Environment.IsDevelopment())
    {
        app.UseHangfireDashboard("/admin/hangfire");
    }

    // Đăng ký Recurring Job cho Zalo OA token refresh (chạy mỗi giờ)
    try
    {
        using var scope = app.Services.CreateScope();
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        recurringJobManager.AddOrUpdate<RealSync.Services.Publishing.ZaloTokenRefreshService>(
            "zalo-token-refresh",
            service => service.RefreshExpiringTokensAsync(CancellationToken.None),
            Cron.Hourly());
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Failed to register Hangfire recurring job for Zalo Token Refresh");
    }

    app.MapControllers().RequireRateLimiting("GlobalPolicy");
    app.MapHealthChecks("/health");

    app.MapGet("/storage/health", async (RealSync.Core.Interfaces.IFileStorageService fileStorageService, CancellationToken ct) =>
    {
        var health = await fileStorageService.CheckHealthAsync(ct);
        return Results.Ok(new
        {
            status = health.Status,
            storageAvailable = health.StorageAvailable,
            freeSpaceBytes = health.FreeSpaceBytes
        });
    }).AllowAnonymous();

    Log.Information("🚀 RealSync API started successfully");
    app.Run();
}
catch (Exception ex) when (ex.GetType().Name != "HostAbortedException")
{
    Log.Fatal(ex, "❌ Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
