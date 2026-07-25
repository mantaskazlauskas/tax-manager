using FluentValidation;

namespace TaxManager.Application.Commands;

public class UpdateTaxRecordCommandValidator : AbstractValidator<UpdateTaxRecordCommand>
{
    public UpdateTaxRecordCommandValidator()
    {
        RuleFor(x => x.TaxRecordId).GreaterThan(0);
        RuleFor(x => x.Rate).GreaterThanOrEqualTo(0).WithMessage("Rate must not be negative.");
    }
}
