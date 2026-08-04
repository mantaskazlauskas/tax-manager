namespace TaxManager.Application.Caching;

public static class TaxRecordCacheKeys
{
    public static string NormalizeScope(string municipalityName) => municipalityName.Trim().ToLowerInvariant();

    public static string GenerationKey(string scope) => $"tax-rate-gen:{scope}";

    public static string DataKey(string scope, string keySuffix, string generation) =>
        $"tax-rate:{scope}:{keySuffix}:{generation}";
}
