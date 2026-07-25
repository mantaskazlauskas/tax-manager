using Microsoft.EntityFrameworkCore;
using TaxManager.Application.Abstractions;
using TaxManager.Domain.Entities;
using TaxManager.Infrastructure.Persistence;

namespace TaxManager.Infrastructure.Repositories;

public class TaxRecordRepository(TaxManagerDbContext dbContext) : ITaxRecordRepository
{
    public Task<TaxRecord?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.TaxRecords.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TaxRecord>> GetByMunicipalityIdAsync(int municipalityId, CancellationToken cancellationToken) =>
        await dbContext.TaxRecords.Where(t => t.MunicipalityId == municipalityId).ToListAsync(cancellationToken);

    public async Task AddAsync(TaxRecord taxRecord, CancellationToken cancellationToken) =>
        await dbContext.TaxRecords.AddAsync(taxRecord, cancellationToken);
}
