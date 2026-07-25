using FluentValidation;

namespace TaxManager.Application.Commands;

public class AddTaxRecordCommandValidator : AbstractValidator<AddTaxRecordCommand>
{
    public AddTaxRecordCommandValidator()
    {
        RuleFor(x => x.MunicipalityName).NotEmpty();
        RuleFor(x => x.Rate).GreaterThanOrEqualTo(0).WithMessage("Rate must not be negative.");
    }
}
