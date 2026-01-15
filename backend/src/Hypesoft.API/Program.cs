using Hypesoft.API.Extensions;
using Serilog;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Hypesoft.API")
    .WriteTo.Console()
    .WriteTo.File("logs/hypesoft-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Iniciando Hypesoft API");

    var builder = WebApplication.CreateBuilder(args);

    // Usar Serilog como provider de logging
    builder.Host.UseSerilog();

    // Registrar serviços (Extension Methods - SOLID)
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApiServices(builder.Configuration);

    // Configurar Kestrel para Docker
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(80);
    });

    var app = builder.Build();

    // Configurar pipeline de middlewares
    app.ConfigureMiddlewares();

    Log.Information("Hypesoft API iniciada com sucesso");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação falhou ao iniciar");
    throw;
}
finally
{
    Log.CloseAndFlush();
}