using System.Text;
using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RealSync.Api.Filters;
using RealSync.Api.HostedServices;
using RealSync.Api.Services;
using RealSync.Core.Interfaces;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Data.Repositories;
using RealSync.Services.Implementations;
using RealSync.Services.Options;
using RealSync.Services.Publishing;
using RealSync.Core.Interfaces.Media;
using RealSync.Services.Media;

namespace RealSync.Api.Extensions;

/// <summary>
/// Extension methods cho đăng ký DI services.
/// Tập trung tất cả service registration ở đây.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký tất cả application services.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Infrastructure
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        // Services
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ICrmAnalyticsService, CrmAnalyticsService>();
        services.AddScoped<IFollowUpReminderService, FollowUpReminderService>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICrawlerService, CrawlerService>();
        services.AddScoped<IPublicationService, PublicationService>();

        // Posting services
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IPostChannelService, PostChannelService>();
        services.AddScoped<IPostAnalyticsService, PostAnalyticsService>();
        services.AddScoped<IPostScheduleService, PostScheduleService>();
        services.AddSingleton(new HttpClient()); // built-in, không cần cài gói
        services.AddScoped<IAIContentService, AIContentService>();

        // Publishing services
        services.AddScoped<IConnectedAccountService, ConnectedAccountService>();
        services.AddScoped<IConnectorResolver, ConnectorResolver>();
        services.AddScoped<ICredentialVault, LocalCredentialVault>();
        services.AddScoped<IPublicationOrchestrator, PublicationOrchestrator>();

        // Video and Storage Services
        services.AddScoped<IVideoProjectService, VideoProjectService>();
        services.AddScoped<IVideoRenderService, FfmpegVideoRenderService>();
        services.AddHttpClient<IVideoGenerationProvider, VeoVideoProvider>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5);
        });
        services.AddHttpClient<IAITextProvider, GeminiAIProvider>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Publishing Engine (Connectors)
        services.AddScoped<IPublishingConnector, WebsiteConnector>();
        services.AddScoped<IPublishingConnector, PortalConnector>();

        services.AddScoped<FacebookGroupConnector>();
        services.AddScoped<IPublishingConnector, FacebookGroupConnector>(sp => sp.GetRequiredService<FacebookGroupConnector>());

        services.AddHttpClient<InstagramProfessionalConnector>(client =>
        {
            client.BaseAddress = new Uri("https://graph.facebook.com/v25.0/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });
        services.AddScoped<IPublishingConnector, InstagramProfessionalConnector>(sp =>
            sp.GetRequiredService<InstagramProfessionalConnector>());

        services.AddHttpClient<MetaPageConnector>(client =>
        {
            client.BaseAddress = new Uri("https://graph.facebook.com/v25.0/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });
        services.AddScoped<IPublishingConnector, MetaPageConnector>(sp =>
            sp.GetRequiredService<MetaPageConnector>());

        services.AddHttpClient<TikTokConnector>(client =>
        {
            client.BaseAddress = new Uri("https://open.tiktokapis.com/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });
        services.AddScoped<IPublishingConnector, TikTokConnector>(sp =>
            sp.GetRequiredService<TikTokConnector>());

        services.AddHttpClient<YouTubeConnector>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(10);
        });
        services.AddScoped<IPublishingConnector, YouTubeConnector>(sp =>
            sp.GetRequiredService<YouTubeConnector>());

        services.AddHttpClient<ZaloOAConnector>(client =>
        {
            client.BaseAddress = new Uri("https://openapi.zalo.me/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddScoped<IPublishingConnector, ZaloOAConnector>(sp =>
            sp.GetRequiredService<ZaloOAConnector>());

        // Token & OAuth services
        services.AddHttpClient<TikTokOAuthService>(client =>
        {
            client.BaseAddress = new Uri("https://open.tiktokapis.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddScoped<TikTokTokenRefreshService>();
        services.AddScoped<ZaloTokenRefreshService>();

        // Cache services
        services.AddMemoryCache();

        // File storage
        services.AddScoped<R2FileStorageService>();

        // FluentValidation — auto-scan tất cả validators từ Shared assembly
        services.AddValidatorsFromAssemblyContaining<RealSync.Shared.Validators.Auth.LoginRequestValidator>();
        services.AddHostedService<FollowUpReminderBackgroundService>();

        return services;
    }

    /// <summary>
    /// Cấu hình Controllers với ValidationFilter global.
    /// </summary>
    public static IMvcBuilder AddControllersWithValidation(this IServiceCollection services)
    {
        return services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        })
        .AddJsonOptions(json =>
        {
            json.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            json.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            json.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
    }

    /// <summary>
    /// Cấu hình JWT Authentication.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
            options.AddPolicy("AgentOrAbove", policy => policy.RequireRole("Admin", "Manager", "Agent"));
        });

        return services;
    }

    /// <summary>
    /// Cấu hình Swagger với JWT auth header.
    /// </summary>
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RealSync API",
                Version = "v1",
                Description = "Real Estate Data & Content Operating System API",
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Nhập JWT token. Ví dụ: eyJhbGciOiJIUzI1NiIs..."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Cấu hình CORS cho frontend.
    /// </summary>
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:5173",  // Vite dev
                        "http://localhost:5174",  // Vite dev fallback 1
                        "http://localhost:5175",  // Vite dev fallback 2
                        "http://localhost:3000",  // Docker frontend
                        "https://realsync.vn")    // Production
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Cß║Ñu h├¼nh Hangfire vß╗¢i SqlServer storage.
    /// </summary>
    public static IServiceCollection AddHangfireServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromSeconds(15),
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

        services.AddHangfireServer();

        return services;
    }
}