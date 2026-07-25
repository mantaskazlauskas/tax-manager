namespace TaxManager.Domain.Exceptions;

/// <summary>
/// Thrown when a tax record's date range doesn't match what its <see cref="Enums.TaxPeriodType"/>
/// requires - see the range-length rule documented on <see cref="Entities.TaxRecord"/>.
/// </summary>
public class InvalidTaxPeriodRangeException(string message) : DomainException(message);
