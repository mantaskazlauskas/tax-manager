using FluentValidation;

namespace TaxManager.Application.Queries;

public class GetTaxRateQueryValidator : AbstractValidator<GetTaxRateQuery>
{
    public GetTaxRateQueryValidator()
    {
        RuleFor(x => x.MunicipalityName).NotEmpty();
    }
}
