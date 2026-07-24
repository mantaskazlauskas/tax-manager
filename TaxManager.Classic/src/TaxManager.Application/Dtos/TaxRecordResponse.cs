using TaxManager.Domain.Enums;

namespace TaxManager.Application.Dtos;

public record TaxRecordResponse(
    Guid Id,
    string MunicipalityName,
    TaxPeriodType PeriodType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Rate);
