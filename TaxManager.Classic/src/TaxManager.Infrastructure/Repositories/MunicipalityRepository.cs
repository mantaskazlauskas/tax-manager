using Microsoft.EntityFrameworkCore;
using TaxManager.Application.Abstractions;
using TaxManager.Domain.Entities;
using TaxManager.Infrastructure.Persistence;

namespace TaxManager.Infrastructure.Repositories;

public class MunicipalityRepository(TaxManagerDbContext dbContext) : IMunicipalityRepository
{
    public Task<Municipality?> GetByNameAsync(string name, CancellationToken cancellationToken) =>
        dbContext.Municipalities.FirstOrDefaultAsync(m => m.Name.ToLower() == name.ToLower(), cancellationToken);

    public Task<Municipality?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Municipalities.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task AddAsync(Municipality municipality, CancellationToken cancellationToken) =>
        await dbContext.Municipalities.AddAsync(municipality, cancellationToken);
}
