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

    // JWT Authentication
    builder.Services.AddJwtAuthentication(builder.Configuration);

    // CORS
    builder.Services.AddCorsPolicy(builder.Configuration);

    // Controllers with FluentValidation filter
    builder.Services.AddControllersWithValidation();

    // Swagger with JWT
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerWithJwt();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<RealSyncDbContext>("database");

    var app = builder.Build();

    // ===== Database Migration & Seed (Development only) =====
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RealSyncDbContext>();
        await dbContext.Database.MigrateAsync();
        await DataSeeder.SeedAsync(dbContext);
    }

    // ===== Middleware Pipeline =====
    app.UseSerilogRequestLogging();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

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

    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("🚀 RealSync API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
