using TaxManager.Domain.Enums;

namespace TaxManager.Application.Dtos;

public record TaxRateResponse(
    string Municipality,
    DateOnly Date,
    decimal Rate,
    TaxPeriodType PeriodType);
