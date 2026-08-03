namespace TaxManager.Application.Caching;

public static class TaxRecordCacheKeys
{
    public static string BuildKey(string municipalityName) =>
        $"tax-records:{municipalityName.Trim().ToLowerInvariant()}";
}
