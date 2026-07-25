using TaxManager.Domain.Enums;

namespace TaxManager.Application.Dtos;

public record TaxRecordResponse(
    int Id,
    string MunicipalityName,
    TaxPeriodType PeriodType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Rate);
