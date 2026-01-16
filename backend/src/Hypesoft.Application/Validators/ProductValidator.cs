using FluentValidation;

namespace Hypesoft.Application.Validators
{
    public class ProductValidator : AbstractValidator<object>
    {
        public ProductValidator()
        {
            RuleFor(x => x).NotNull();
        }
    }
}