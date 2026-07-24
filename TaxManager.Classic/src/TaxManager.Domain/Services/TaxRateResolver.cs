using TaxManager.Domain.Entities;

namespace TaxManager.Domain.Services;

/// <summary>
/// Resolves which tax record applies to a municipality on a given date. Pure domain logic: no I/O,
/// so callers (Application layer) are responsible for loading the candidate records first.
/// </summary>
public static class TaxRateResolver
{
    /// <summary>
    /// Among the records whose date range covers <paramref name="date"/>, returns the one with the
    /// most specific <see cref="Enums.TaxPeriodType"/> (Daily beats Weekly beats Monthly beats
    /// Yearly), or null if none apply.
    /// </summary>
    public static TaxRecord? Resolve(IEnumerable<TaxRecord> taxRecords, DateOnly date) =>
        taxRecords
            .Where(record => record.CoversDate(date))
            .OrderByDescending(record => (int)record.PeriodType)
            .FirstOrDefault();
}
