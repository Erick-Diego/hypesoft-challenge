using FluentValidation;

namespace Hypesoft.Application.Validators
{
    public class ProductValidator : AbstractValidator<object> // substitua pelo DTO real depois
    {
        public ProductValidator()
        {
            RuleFor(x => x).NotNull();
        }
    }
}