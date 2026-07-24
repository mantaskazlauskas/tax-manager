using TaxManager.Domain.Entities;

namespace TaxManager.Application.Abstractions;

public interface ITaxRecordRepository
{
    Task<TaxRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TaxRecord>> GetByMunicipalityIdAsync(Guid municipalityId, CancellationToken cancellationToken);

    Task AddAsync(TaxRecord taxRecord, CancellationToken cancellationToken);
}
