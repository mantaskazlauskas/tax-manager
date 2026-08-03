namespace TaxManager.Application.Options;

public class CachingOptions
{
    public int TaxRatesSlidingExpirationMinutes { get; set; } = 10;
}
