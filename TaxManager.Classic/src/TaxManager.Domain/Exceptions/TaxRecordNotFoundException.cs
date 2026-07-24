namespace TaxManager.Domain.Exceptions;

public class TaxRecordNotFoundException(Guid taxRecordId)
    : DomainException($"Tax record '{taxRecordId}' was not found.");
