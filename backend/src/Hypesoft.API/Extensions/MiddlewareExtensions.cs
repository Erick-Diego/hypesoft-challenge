using Serilog;

namespace Hypesoft.API.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication ConfigureMiddlewares(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
                diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress);
            };
        });

        app.UseSecurityHeaders();

        app.UseGlobalExceptionHandler();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hypesoft API v1");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "Hypesoft API - Documentação";
            c.DisplayRequestDuration();
        });

        app.UseCors();
        app.UseRateLimiter();
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false
        });

        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Aplicação iniciada em: http://localhost:5000");
        logger.LogInformation("Swagger disponível em: http://localhost:5000/swagger");
        logger.LogInformation("Health Check disponível em: http://localhost:5000/health");

        return app;
    }

    private static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
            
            await next();
        });

        return app;
    }

    private static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var exceptionHandlerPathFeature = context.Features
                    .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature?.Error;

                Log.Error(exception, "Erro não tratado: {ErrorMessage}", exception?.Message);

                var response = new
                {
                    error = "Ocorreu um erro interno no servidor",
                    message = app.ApplicationServices.GetRequiredService<IHostEnvironment>().IsDevelopment() 
                        ? exception?.Message 
                        : null,
                    timestamp = DateTime.UtcNow
                };

                await context.Response.WriteAsJsonAsync(response);
            });
        });

        return app;
    }
}