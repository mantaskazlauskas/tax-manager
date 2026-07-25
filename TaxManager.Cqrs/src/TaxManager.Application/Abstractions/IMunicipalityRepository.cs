using TaxManager.Domain.Entities;

namespace TaxManager.Application.Abstractions;

public interface IMunicipalityRepository
{
    /// <summary>Case-insensitive lookup by name.</summary>
    Task<Municipality?> GetByNameAsync(string name, CancellationToken cancellationToken);

    Task<Municipality?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task AddAsync(Municipality municipality, CancellationToken cancellationToken);
}
