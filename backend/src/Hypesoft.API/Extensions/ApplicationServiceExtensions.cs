using FluentValidation;
using FluentValidation.AspNetCore;
using Hypesoft.Application.Validators;

namespace Hypesoft.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(ProductValidator).Assembly);
        });

        services.AddAutoMapper(typeof(ProductValidator).Assembly);

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<ProductValidator>();

        return services;
    }
}