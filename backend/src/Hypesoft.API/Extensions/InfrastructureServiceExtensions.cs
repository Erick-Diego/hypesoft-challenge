using Hypesoft.Infrastructure.Configurations;
using Hypesoft.Infrastructure.Services;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Repositories;

namespace Hypesoft.API.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(
            configuration.GetSection("MongoDbSettings")
        );
        services.AddSingleton<MongoDbService>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        return services;
    }
}