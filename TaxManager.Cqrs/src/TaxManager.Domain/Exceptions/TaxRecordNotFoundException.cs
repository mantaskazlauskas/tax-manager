namespace TaxManager.Domain.Exceptions;

public class TaxRecordNotFoundException(int taxRecordId)
    : DomainException($"Tax record '{taxRecordId}' was not found.");
