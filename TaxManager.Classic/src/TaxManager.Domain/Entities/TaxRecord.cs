using TaxManager.Domain.Enums;
using TaxManager.Domain.Exceptions;

namespace TaxManager.Domain.Entities;

/// <summary>
/// A tax rate that applies to a municipality for an inclusive date range. <see cref="PeriodType"/>
/// also constrains the range's length (Daily = 1 day, Weekly = 7 days, Monthly/Yearly = one
/// calendar month/year from <see cref="StartDate"/>) - <see cref="Services.TaxRateResolver"/> then
/// uses it to break ties when several records cover the same date.
/// </summary>
public class TaxRecord
{
    public int Id { get; private set; }
    public int MunicipalityId { get; private set; }
    public TaxPeriodType PeriodType { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public decimal Rate { get; private set; }

    private TaxRecord() { }

    public TaxRecord(int municipalityId, TaxPeriodType periodType, DateOnly startDate, DateOnly endDate, decimal rate)
    {
        MunicipalityId = municipalityId;
        SetValues(periodType, startDate, endDate, rate);
    }

    public void Update(TaxPeriodType periodType, DateOnly startDate, DateOnly endDate, decimal rate) =>
        SetValues(periodType, startDate, endDate, rate);

    public bool CoversDate(DateOnly date) => date >= StartDate && date <= EndDate;

    public bool OverlapsWith(DateOnly startDate, DateOnly endDate) =>
        StartDate <= endDate && startDate <= EndDate;

    private void SetValues(TaxPeriodType periodType, DateOnly startDate, DateOnly endDate, decimal rate)
    {
        EnsureValidRange(periodType, startDate, endDate);

        if (rate < 0)
        {
            throw new ArgumentException("Rate must not be negative.", nameof(rate));
        }

        PeriodType = periodType;
        StartDate = startDate;
        EndDate = endDate;
        Rate = rate;
    }

    /// <summary>
    /// Validates that <paramref name="endDate"/> matches the length <paramref name="periodType"/>
    /// implies (see class summary). Exposed so callers can fail fast on obviously-invalid input
    /// before touching the database - the constructor/<see cref="Update"/> still enforce it too,
    /// since that's the only path that can't be bypassed.
    /// </summary>
    public static void EnsureValidRange(TaxPeriodType periodType, DateOnly startDate, DateOnly endDate)
    {
        if (startDate > endDate)
        {
            throw new InvalidTaxPeriodRangeException("Start date must not be after end date.");
        }

        var expectedEndDate = ExpectedEndDate(periodType, startDate);
        if (endDate != expectedEndDate)
        {
            throw new InvalidTaxPeriodRangeException(
                $"A {periodType} record starting {startDate:yyyy-MM-dd} must end {expectedEndDate:yyyy-MM-dd}, but {endDate:yyyy-MM-dd} was given.");
        }
    }

    private static DateOnly ExpectedEndDate(TaxPeriodType periodType, DateOnly startDate) => periodType switch
    {
        TaxPeriodType.Daily => startDate,
        TaxPeriodType.Weekly => startDate.AddDays(6),
        TaxPeriodType.Monthly => startDate.AddMonths(1).AddDays(-1),
        TaxPeriodType.Yearly => startDate.AddYears(1).AddDays(-1),
        _ => throw new ArgumentOutOfRangeException(nameof(periodType), periodType, "Unknown tax period type.")
    };
}
