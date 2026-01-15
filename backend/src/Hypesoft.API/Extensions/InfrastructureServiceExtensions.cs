using Hypesoft.Infrastructure.Configurations;
using Hypesoft.Infrastructure.Services;

namespace Hypesoft.API.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // MongoDB
        services.Configure<MongoDbSettings>(
            configuration.GetSection("MongoDbSettings")
        );
        services.AddSingleton<MongoDbService>();

        // Aqui você adicionaria repositórios, serviços externos, etc.
        // services.AddScoped<IProductRepository, ProductRepository>();
        // services.AddScoped<ICategoryRepository, CategoryRepository>();

        return services;
    }
}