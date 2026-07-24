using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaxManager.Application.Abstractions;
using TaxManager.Infrastructure.Persistence;
using TaxManager.Infrastructure.Repositories;

namespace TaxManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Resolved lazily from DI (rather than captured from `configuration` here) so overrides
        // applied after this call - e.g. WebApplicationFactory's test configuration - still win.
        services.AddDbContext<TaxManagerDbContext>((sp, options) =>
        {
            var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("TaxManagerDb")
                ?? throw new InvalidOperationException("Connection string 'TaxManagerDb' is not configured.");

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TaxManagerDbContext>());
        services.AddScoped<IMunicipalityRepository, MunicipalityRepository>();
        services.AddScoped<ITaxRecordRepository, TaxRecordRepository>();

        return services;
    }
}
