namespace TaxManager.Domain.Exceptions;

/// <summary>
/// Thrown when a tax record's date range overlaps an existing record of the same
/// <see cref="Enums.TaxPeriodType"/> for the same municipality. Overlaps across different period
/// types are allowed and expected - see assumption note in TaxRateResolver.
/// </summary>
public class OverlappingTaxPeriodException(string municipalityName)
    : DomainException($"A tax record of the same period type already exists for '{municipalityName}' that overlaps the given date range.");
