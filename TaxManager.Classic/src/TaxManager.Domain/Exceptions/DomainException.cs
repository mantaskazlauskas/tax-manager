namespace TaxManager.Domain.Exceptions;

/// <summary>
/// Base type for expected domain-rule violations. The API's global exception handler maps these
/// (and subclasses) to specific 4xx responses; anything else is treated as unexpected and hidden
/// from the caller.
/// </summary>
public abstract class DomainException(string message) : Exception(message);
