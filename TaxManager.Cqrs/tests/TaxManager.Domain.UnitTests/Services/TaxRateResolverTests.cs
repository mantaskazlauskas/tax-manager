using TaxManager.Domain.Entities;
using TaxManager.Domain.Enums;
using TaxManager.Domain.Services;

namespace TaxManager.Domain.UnitTests.Services;

public class TaxRateResolverTests
{
    private const int MunicipalityId = 1;

    // Reproduces the Copenhagen example from requirements.md:
    //  - Yearly 0.2   for 2024-01-01..2024-12-31
    //  - Monthly 0.4  for 2024-05-01..2024-05-31
    //  - Daily 0.1    on 2024-01-01 and 2024-12-25
    private static List<TaxRecord> CopenhagenSchedule() =>
    [
        new TaxRecord(MunicipalityId, TaxPeriodType.Yearly, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31), 0.2m),
        new TaxRecord(MunicipalityId, TaxPeriodType.Monthly, new DateOnly(2024, 5, 1), new DateOnly(2024, 5, 31), 0.4m),
        new TaxRecord(MunicipalityId, TaxPeriodType.Daily, new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 1), 0.1m),
        new TaxRecord(MunicipalityId, TaxPeriodType.Daily, new DateOnly(2024, 12, 25), new DateOnly(2024, 12, 25), 0.1m)
    ];

    [Theory]
    [InlineData(2024, 1, 1, 0.1)]   // Daily beats Yearly
    [InlineData(2024, 3, 16, 0.2)]  // Only Yearly applies
    [InlineData(2024, 5, 2, 0.4)]   // Monthly beats Yearly
    [InlineData(2024, 7, 10, 0.2)]  // Only Yearly applies
    public void Resolve_MatchesRequirementsExampleTable(int year, int month, int day, decimal expectedRate)
    {
        var result = TaxRateResolver.Resolve(CopenhagenSchedule(), new DateOnly(year, month, day));

        Assert.NotNull(result);
        Assert.Equal(expectedRate, result.Rate);
    }

    [Fact]
    public void Resolve_ReturnsNull_WhenNoRecordCoversTheDate()
    {
        var records = new List<TaxRecord>
        {
            new(MunicipalityId, TaxPeriodType.Yearly, new DateOnly(2023, 1, 1), new DateOnly(2023, 12, 31), 0.2m)
        };

        var result = TaxRateResolver.Resolve(records, new DateOnly(2024, 1, 1));

        Assert.Null(result);
    }

    [Fact]
    public void Resolve_ReturnsNull_WhenNoRecordsExist()
    {
        var result = TaxRateResolver.Resolve([], new DateOnly(2024, 1, 1));

        Assert.Null(result);
    }

    [Theory]
    [InlineData(TaxPeriodType.Daily)]
    [InlineData(TaxPeriodType.Weekly)]
    [InlineData(TaxPeriodType.Monthly)]
    public void Resolve_PrefersMoreSpecificPeriod_OverYearly(TaxPeriodType morePrecisePeriod)
    {
        var date = new DateOnly(2024, 6, 15);
        var end = morePrecisePeriod switch
        {
            TaxPeriodType.Daily => date,
            TaxPeriodType.Weekly => date.AddDays(6),
            TaxPeriodType.Monthly => date.AddMonths(1).AddDays(-1),
            _ => throw new ArgumentOutOfRangeException(nameof(morePrecisePeriod))
        };
        var records = new List<TaxRecord>
        {
            new(MunicipalityId, TaxPeriodType.Yearly, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31), 0.2m),
            new(MunicipalityId, morePrecisePeriod, date, end, 0.9m)
        };

        var result = TaxRateResolver.Resolve(records, date);

        Assert.NotNull(result);
        Assert.Equal(0.9m, result.Rate);
        Assert.Equal(morePrecisePeriod, result.PeriodType);
    }

    [Theory]
    [InlineData(2024, 1, 1, true)]   // range start (inclusive)
    [InlineData(2024, 1, 31, true)]  // range end (inclusive)
    [InlineData(2023, 12, 31, false)] // just before range
    [InlineData(2024, 2, 1, false)]   // just after range
    public void Resolve_TreatsRecordRangeAsInclusiveOnBothEnds(int year, int month, int day, bool expectedMatch)
    {
        var records = new List<TaxRecord>
        {
            new(MunicipalityId, TaxPeriodType.Monthly, new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31), 0.4m)
        };

        var result = TaxRateResolver.Resolve(records, new DateOnly(year, month, day));

        Assert.Equal(expectedMatch, result is not null);
    }
}
