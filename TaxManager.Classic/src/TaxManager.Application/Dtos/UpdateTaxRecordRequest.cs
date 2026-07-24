using TaxManager.Domain.Enums;

namespace TaxManager.Application.Dtos;

public record UpdateTaxRecordRequest(
    TaxPeriodType PeriodType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Rate);
