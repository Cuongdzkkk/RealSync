using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RealSync.Api.Filters;
using RealSync.Api.Services;
using RealSync.Core.Interfaces;
using RealSync.Data.Repositories;
using RealSync.Services.Implementations;
using RealSync.Services.Options;

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
        services.AddScoped<IFileStorageService, R2FileStorageService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ICrmAnalyticsService, CrmAnalyticsService>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<ICustomerService, CustomerService>();

        // Posting services
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IPostChannelService, PostChannelService>();
        services.AddScoped<IPostAnalyticsService, PostAnalyticsService>();
        services.AddScoped<IPostScheduleService, PostScheduleService>();
        services.AddSingleton(new HttpClient()); // built-in, không cần cài gói
        services.AddScoped<IAIContentService, AIContentService>();


        // FluentValidation — auto-scan tất cả validators từ Shared assembly
        services.AddValidatorsFromAssemblyContaining<RealSync.Shared.Validators.Auth.LoginRequestValidator>();

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
                        "http://localhost:3000",  // Docker frontend
                        "https://realsync.vn")    // Production
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
