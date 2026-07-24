using TaxManager.Domain.Enums;

namespace TaxManager.Application.Dtos;

public record CreateTaxRecordRequest(
    string MunicipalityName,
    TaxPeriodType PeriodType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Rate);
