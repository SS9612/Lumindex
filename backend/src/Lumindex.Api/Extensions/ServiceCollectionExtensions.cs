using System.Text;
using Lumindex.Api.Authentication;
using Lumindex.Api.Common;
using Lumindex.Application.Authentication;
using Lumindex.Application.Common.Interfaces;
using Lumindex.Infrastructure.Persistence;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Lumindex.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLumindexOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddLumindexCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5173" };

        services.AddCors(options =>
        {
            options.AddPolicy("LumindexFrontend", policy =>
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        return services;
    }

    public static IServiceCollection AddLumindexAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var key = string.IsNullOrWhiteSpace(jwt.SigningKey)
            ? "dev-only-change-me-dev-only-change-me-32+chars"
            : jwt.SigningKey;

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorization();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddExceptionHandler<ValidationExceptionHandler>();

        return services;
    }

    public static IServiceCollection AddLumindexHealthChecks(this IServiceCollection services, IConfiguration _)
    {
        services
            .AddHealthChecks()
            .AddDbContextCheck<LumindexDbContext>(
                name: "database",
                tags: new[] { "ready", "db" });

        return services;
    }

    public static IServiceCollection AddLumindexOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApi();
        return services;
    }

    public static IServiceCollection AddLumindexBackgroundJobs(this IServiceCollection services, IConfiguration _)
    {
        // In-memory storage keeps local dev/CI dependency-free. Swap for Hangfire.PostgreSql (or
        // SQL Server/Redis) in production so jobs survive restarts.
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseInMemoryStorage());

        services.AddHangfireServer();

        return services;
    }
}
