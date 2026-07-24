namespace TaxManager.Domain.Exceptions;

/// <summary>
/// Thrown when no tax record covers the requested date. Modelled as "not found" (404) rather than
/// a 0 rate, since "no rate configured" and "a 0% rate" are semantically different.
/// </summary>
public class TaxRateNotFoundException(string municipalityName, DateOnly date)
    : DomainException($"No tax rate is configured for municipality '{municipalityName}' on {date:yyyy-MM-dd}.");
