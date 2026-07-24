using TaxManager.Domain.Enums;

namespace TaxManager.Application.Dtos;

/// <summary>Request body shape for PUT /api/tax-records/{id} - the id itself comes from the route.</summary>
public record UpdateTaxRecordRequest(
    TaxPeriodType PeriodType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Rate);
