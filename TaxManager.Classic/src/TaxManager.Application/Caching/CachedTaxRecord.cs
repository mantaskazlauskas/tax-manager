using TaxManager.Domain.Enums;

namespace TaxManager.Application.Caching;

/// <summary>
/// Serializable stand-in for <see cref="Domain.Entities.TaxRecord"/> - the entity's setters are
/// private (EF Core only), so it can't round-trip through System.Text.Json directly. Id is
/// intentionally omitted: cached records are only ever fed into TaxRateResolver, which doesn't use it.
/// </summary>
internal sealed record CachedTaxRecord(int MunicipalityId, TaxPeriodType PeriodType, DateOnly StartDate, DateOnly EndDate, decimal Rate);
