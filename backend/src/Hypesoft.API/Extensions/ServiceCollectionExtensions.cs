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
using Swashbuckle.AspNetCore.Annotations;
using HealthChecks.System;
using HealthChecks.Uris;

namespace Hypesoft.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        
        services.AddSwaggerConfiguration(configuration);
        services.AddAuthenticationConfiguration(configuration);
        services.AddCorsConfiguration();
        services.AddRateLimitingConfiguration(configuration);
        services.AddHealthChecksConfiguration(configuration);
        
        return services;
    }

    private static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services, IConfiguration configuration)
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
                    Email = "dev@hypesoft.com",
                    Url = new Uri("https://github.com/hypesoft/hypesoft-challenge")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            c.EnableAnnotations();
            
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header usando Bearer scheme. Digite 'Bearer' seguido de espaço e seu token. Exemplo: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
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
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            c.DescribeAllParametersInCamelCase();
            c.OrderActionsBy(apiDesc => apiDesc.RelativePath);
        });

        return services;
    }

    private static IServiceCollection AddAuthenticationConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var keycloakAuthority = configuration["Keycloak:Authority"] 
            ?? throw new InvalidOperationException("Keycloak:Authority não configurado");
        
        var keycloakAudience = configuration["Keycloak:Audience"] 
            ?? throw new InvalidOperationException("Keycloak:Audience não configurado");
        
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        var isDevelopment = environment == "Development";

        var validIssuers = new List<string>
        {
            keycloakAuthority.TrimEnd('/'),
            keycloakAuthority.Replace("keycloak:8080", "localhost:8080").TrimEnd('/'),
            "http://localhost:8080/realms/hypesoft",
            "http://keycloak:8080/realms/hypesoft"
        }.Distinct().ToList();

        var logger = services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
        logger.LogInformation("   Configurando autenticação Keycloak");
        logger.LogInformation("   Authority: {Authority}", keycloakAuthority);
        logger.LogInformation("   Audience: {Audience}", keycloakAudience);
        logger.LogInformation("   Environment: {Environment}", environment);
        logger.LogInformation("   Valid Issuers: {Issuers}", string.Join(", ", validIssuers));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = keycloakAuthority;
                options.Audience = keycloakAudience;
                options.RequireHttpsMetadata = false;
                
                options.BackchannelTimeout = TimeSpan.FromSeconds(30);
                options.RefreshInterval = TimeSpan.FromMinutes(5);
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    
                    ValidIssuers = validIssuers,
                    ValidAudiences = new[] { keycloakAudience, "account" },
                    
                    ClockSkew = isDevelopment
                        ? TimeSpan.FromMinutes(5)
                        : TimeSpan.FromMinutes(2),
                    
                    NameClaimType = "preferred_username",
                    RoleClaimType = "role"
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();
                        
                        var token = context.Request.Headers["Authorization"]
                            .ToString()
                            .Replace("Bearer ", "");
                        
                        if (isDevelopment)
                        {
                            logger.LogError(
                                "   Autenticação falhou\n" +
                                "   Erro: {Error}\n" +
                                "   Endpoint: {Method} {Path}\n" +
                                "   Token (primeiros 50 chars): {Token}\n" +
                                "   Exception Type: {ExceptionType}\n" +
                                "   Inner Exception: {InnerException}",
                                context.Exception.Message,
                                context.Request.Method,
                                context.Request.Path,
                                token.Length > 50 ? token.Substring(0, 50) + "..." : token,
                                context.Exception.GetType().Name,
                                context.Exception.InnerException?.Message ?? "None"
                            );
                        }
                        else
                        {
                            logger.LogWarning(
                                "Autenticação falhou: {Error} - {Method} {Path}",
                                context.Exception.Message,
                                context.Request.Method,
                                context.Request.Path
                            );
                        }
                        
                        return Task.CompletedTask;
                    },
                    
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();
                        
                        if (context.Principal?.Identity is ClaimsIdentity identity)
                        {
                            var issuer = identity.FindFirst("iss")?.Value;
                            var username = identity.FindFirst("preferred_username")?.Value;
                            var userId = identity.FindFirst("sub")?.Value;
                            var clientId = identity.FindFirst("azp")?.Value;
                            
                            var roles = new List<string>();
                            var realmAccessClaim = identity.FindFirst("realm_access");
                            if (realmAccessClaim != null)
                            {
                                try
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
                                                roles.Add(roleValue);
                                            }
                                        }
                                    }
                                }
                                catch (JsonException ex)
                                {
                                    logger.LogWarning("Erro ao parsear realm_access: {Error}", ex.Message);
                                }
                            }
                            
                            logger.LogInformation(
                                "   Token validado com sucesso\n" +
                                "   User: {Username} (ID: {UserId})\n" +
                                "   Client: {ClientId}\n" +
                                "   Issuer: {Issuer}\n" +
                                "   Roles: {Roles}\n" +
                                "   Endpoint: {Method} {Path}",
                                username,
                                userId,
                                clientId,
                                issuer,
                                roles.Any() ? string.Join(", ", roles) : "none",
                                context.Request.Method,
                                context.Request.Path
                            );
                        }
                        
                        return Task.CompletedTask;
                    },
                    
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();
                        
                        logger.LogWarning(
                            "⚠️ Authentication Challenge\n" +
                            "   Error: {Error}\n" +
                            "   Description: {Description}\n" +
                            "   Endpoint: {Method} {Path}",
                            context.Error ?? "none",
                            context.ErrorDescription ?? "none",
                            context.Request.Method,
                            context.Request.Path
                        );
                        
                        return Task.CompletedTask;
                    },
                    
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        
                        if (isDevelopment && !string.IsNullOrEmpty(context.Token))
                        {
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILogger<Program>>();
                            logger.LogDebug("📨 Token recebido (length: {Length})", context.Token.Length);
                        }
                        
                        return Task.CompletedTask;
                    },
                    
                    OnForbidden = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();
                        
                        var username = context.Principal?.FindFirst("preferred_username")?.Value;
                        
                        logger.LogWarning(
                            "   Acesso negado\n" +
                            "   User: {Username}\n" +
                            "   Endpoint: {Method} {Path}",
                            username ?? "unknown",
                            context.Request.Method,
                            context.Request.Path
                        );
                        
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => 
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("admin");
            });
            
            options.AddPolicy("Manager", policy => 
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("manager", "admin");
            });
            
            options.AddPolicy("User", policy => 
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("user", "manager", "admin");
            });

            options.AddPolicy("CanManageProducts", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    context.User.IsInRole("admin") ||
                    context.User.IsInRole("manager")
                );
            });
        });

        return services;
    }

    private static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins(
                    "http://localhost:3000",
                    "http://localhost:5173",
                    "http://localhost:5174"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("X-Pagination", "X-Total-Count"));

            options.AddPolicy("Production", policy =>
                policy.WithOrigins("https://app.hypesoft.com")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        return services;
    }

    private static IServiceCollection AddRateLimitingConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        var isDevelopment = environment == "Development";

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            string GetClientIdentifier(HttpContext context)
            {
                var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (ips.Length > 0)
                    {
                        return ips[0].Trim();
                    }
                }

                var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(realIp))
                {
                    return realIp;
                }

                if (context.User?.Identity?.IsAuthenticated == true)
                {
                    return context.User.Identity.Name ?? "authenticated-user";
                }

                return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            }

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var identifier = GetClientIdentifier(context);
                
                return RateLimitPartition.GetFixedWindowLimiter(identifier, _ => 
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = isDevelopment ? 1000 : 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = isDevelopment ? 100 : 10,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });
            });

            options.AddPolicy("read", context =>
            {
                var identifier = GetClientIdentifier(context);
                
                return RateLimitPartition.GetFixedWindowLimiter(identifier, _ => 
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = isDevelopment ? 500 : 200,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 50
                    });
            });

            options.AddPolicy("write", context =>
            {
                var identifier = GetClientIdentifier(context);
                
                return RateLimitPartition.GetFixedWindowLimiter(identifier, _ => 
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = isDevelopment ? 100 : 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 5
                    });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                
                var retryAfterSeconds = 60.0;
                
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    retryAfterSeconds = retryAfter.TotalSeconds;
                    context.HttpContext.Response.Headers.RetryAfter = 
                        ((int)retryAfterSeconds).ToString();
                }

                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();
                
                var identifier = GetClientIdentifier(context.HttpContext);
                
                logger.LogWarning(
                    "   Rate limit atingido\n" +
                    "   Identifier: {Identifier}\n" +
                    "   Endpoint: {Method} {Path}\n" +
                    "   Retry After: {RetryAfter}s",
                    identifier,
                    context.HttpContext.Request.Method,
                    context.HttpContext.Request.Path,
                    retryAfterSeconds
                );

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "TooManyRequests",
                    message = "Você atingiu o limite de requisições. Por favor, aguarde antes de tentar novamente.",
                    retryAfter = retryAfterSeconds
                }, cancellationToken: cancellationToken);
            };
        });

        return services;
    }

    private static IServiceCollection AddHealthChecksConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        healthChecksBuilder.AddMongoDb(
            sp => 
            {
                var connectionString = configuration["MongoDbSettings:ConnectionString"] 
                    ?? throw new InvalidOperationException("MongoDB ConnectionString não configurado");
                return new MongoDB.Driver.MongoClient(connectionString);
            },
            name: "mongodb",
            timeout: TimeSpan.FromSeconds(3),
            tags: new[] { "db", "mongodb", "ready" }
        );
        var keycloakUrl = configuration["Keycloak:Authority"];
        if (!string.IsNullOrEmpty(keycloakUrl))
        {
            healthChecksBuilder.AddUrlGroup(
                new Uri($"{keycloakUrl}/.well-known/openid-configuration"),
                name: "keycloak",
                timeout: TimeSpan.FromSeconds(5),
                tags: new[] { "auth", "keycloak", "ready" }
            );
        }

        healthChecksBuilder.AddPrivateMemoryHealthCheck(
            maximumMemoryBytes: 1_500_000_000,
            name: "memory",
            tags: new[] { "system", "memory" }
        );

        return services;
    }
}