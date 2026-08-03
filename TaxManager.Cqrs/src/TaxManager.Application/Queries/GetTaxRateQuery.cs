using TaxManager.Application.Caching;
using TaxManager.Application.Dtos;

namespace TaxManager.Application.Queries;

public record GetTaxRateQuery(string MunicipalityName, DateOnly Date) : ICacheableQuery<TaxRateResponse>
{
    public string CacheScope => TaxRecordCacheKeys.NormalizeScope(MunicipalityName);
    public string CacheKeySuffix => Date.ToString("O");
}
