using TaxManager.Domain.Entities;
using TaxManager.Domain.Enums;
using TaxManager.Domain.Exceptions;

namespace TaxManager.Application.Common;

/// <summary>
/// Assumption: overlapping ranges of the *same* period type for the *same* municipality are
/// rejected, since there would be no defined tie-breaker between them. Overlaps across
/// different period types are fine - that's what TaxRateResolver's priority order is for.
/// </summary>
public static class OverlapGuard
{
    public static void EnsureNoOverlap(
        IReadOnlyList<TaxRecord> existingRecords,
        TaxPeriodType periodType,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludeId,
        string municipalityName)
    {
        var hasOverlap = existingRecords.Any(record =>
            record.Id != excludeId &&
            record.PeriodType == periodType &&
            record.OverlapsWith(startDate, endDate));

        if (hasOverlap)
        {
            throw new OverlappingTaxPeriodException(municipalityName);
        }
    }
}
