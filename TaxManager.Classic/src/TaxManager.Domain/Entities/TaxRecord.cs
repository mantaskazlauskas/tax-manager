using TaxManager.Domain.Enums;

namespace TaxManager.Domain.Entities;

/// <summary>
/// A tax rate that applies to a municipality for an inclusive date range. <see cref="PeriodType"/>
/// does not derive the range (a "Weekly" record is just whatever range was supplied) - it only
/// expresses how specific the record is, which <see cref="Services.TaxRateResolver"/> uses to break
/// ties when several records cover the same date.
/// </summary>
public class TaxRecord
{
    public Guid Id { get; private set; }
    public Guid MunicipalityId { get; private set; }
    public TaxPeriodType PeriodType { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public decimal Rate { get; private set; }

    private TaxRecord() { }

    public TaxRecord(Guid municipalityId, TaxPeriodType periodType, DateOnly startDate, DateOnly endDate, decimal rate)
    {
        Id = Guid.NewGuid();
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
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date must not be after end date.", nameof(startDate));
        }

        if (rate < 0)
        {
            throw new ArgumentException("Rate must not be negative.", nameof(rate));
        }

        PeriodType = periodType;
        StartDate = startDate;
        EndDate = endDate;
        Rate = rate;
    }
}
