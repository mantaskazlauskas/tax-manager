using Microsoft.EntityFrameworkCore;
using TaxManager.Application.Abstractions;
using TaxManager.Domain.Entities;

namespace TaxManager.Infrastructure.Persistence;

public class TaxManagerDbContext(DbContextOptions<TaxManagerDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Municipality> Municipalities => Set<Municipality>();
    public DbSet<TaxRecord> TaxRecords => Set<TaxRecord>();

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        await SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaxManagerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
