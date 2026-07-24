namespace TaxManager.Domain.Exceptions;

public class MunicipalityNotFoundException(string municipalityName)
    : DomainException($"Municipality '{municipalityName}' was not found.");
