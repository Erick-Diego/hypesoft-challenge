using FluentValidation;
using FluentValidation.AspNetCore;
using Hypesoft.Application.Validators;

namespace Hypesoft.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR (CQRS)
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(ProductValidator).Assembly);
        });

        // AutoMapper
        services.AddAutoMapper(typeof(ProductValidator).Assembly);

        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<ProductValidator>();

        return services;
    }
}