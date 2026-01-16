using System.Reflection;
using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text.Json;

namespace Hypesoft.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        
        services.AddSwaggerConfiguration();
        services.AddAuthenticationConfiguration(configuration);
        services.AddCorsConfiguration();
        services.AddRateLimitingConfiguration();
        services.AddHealthChecksConfiguration(configuration);
        
        return services;
    }

    private static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Hypesoft API",
                Version = "v1",
                Description = "API de Gestão de Produtos - Clean Architecture + DDD + CQRS",
                Contact = new OpenApiContact
                {
                    Name = "Hypesoft Team",
                    Email = "dev@hypesoft.com"
                }
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header. Exemplo: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    private static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var keycloakConfig = configuration.GetSection("Keycloak");
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = keycloakConfig["Authority"];
                options.Audience = keycloakConfig["Audience"];
                options.RequireHttpsMetadata = false;
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = "role"
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.Principal?.Identity is ClaimsIdentity identity)
                        {
                            var realmAccessClaim = identity.FindFirst("realm_access");
                            if (realmAccessClaim != null)
                            {
                                using var doc = JsonDocument.Parse(realmAccessClaim.Value);
                                if (doc.RootElement.TryGetProperty("roles", out var rolesElement))
                                {
                                    foreach (var role in rolesElement.EnumerateArray())
                                    {
                                        var roleValue = role.GetString();
                                        if (!string.IsNullOrEmpty(roleValue))
                                        {
                                            identity.AddClaim(new Claim("role", roleValue));
                                        }
                                    }
                                }
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole("admin"));
            options.AddPolicy("Manager", policy => policy.RequireRole("manager", "admin"));
            options.AddPolicy("User", policy => policy.RequireRole("user", "manager", "admin"));
        });

        return services;
    }

    private static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials());
        });

        return services;
    }

    private static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var userId = context.User?.Identity?.Name 
                    ?? context.Connection.RemoteIpAddress?.ToString() 
                    ?? "anonymous";
                
                return RateLimitPartition.GetFixedWindowLimiter(userId, _ => 
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });

            options.AddPolicy("create", context =>
            {
                var userId = context.User?.Identity?.Name 
                    ?? context.Connection.RemoteIpAddress?.ToString() 
                    ?? "anonymous";
                
                return RateLimitPartition.GetFixedWindowLimiter(userId, _ => 
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }

    private static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddMongoDb(
                sp => 
                {
                    var connectionString = configuration["MongoDbSettings:ConnectionString"] ?? "mongodb://localhost:27017";
                    return new MongoDB.Driver.MongoClient(connectionString);
                },
                name: "mongodb",
                timeout: TimeSpan.FromSeconds(3),
                tags: new[] { "db", "mongodb", "ready" }
            );

        return services;
    }
}