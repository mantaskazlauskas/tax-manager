using FluentValidation;

namespace TaxManager.Application.Commands;

public class UpdateTaxRecordCommandValidator : AbstractValidator<UpdateTaxRecordCommand>
{
    public UpdateTaxRecordCommandValidator()
    {
        RuleFor(x => x.TaxRecordId).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must not be before start date.");
        RuleFor(x => x.Rate).GreaterThanOrEqualTo(0).WithMessage("Rate must not be negative.");
    }
}
