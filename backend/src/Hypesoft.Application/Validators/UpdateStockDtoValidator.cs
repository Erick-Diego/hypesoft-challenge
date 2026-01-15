using FluentValidation;
using Hypesoft.Application.DTOs;

namespace Hypesoft.Application.Validators;

public class UpdateStockDtoValidator : AbstractValidator<UpdateStockDto>
{
    public UpdateStockDtoValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantidade não pode ser negativa");
    }
}