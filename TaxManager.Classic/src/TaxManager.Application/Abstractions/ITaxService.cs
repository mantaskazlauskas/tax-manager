using TaxManager.Application.Dtos;

namespace TaxManager.Application.Abstractions;

public interface ITaxService
{
    Task<TaxRecordResponse> AddTaxRecordAsync(CreateTaxRecordRequest request, CancellationToken cancellationToken);

    Task<TaxRecordResponse> UpdateTaxRecordAsync(Guid taxRecordId, UpdateTaxRecordRequest request, CancellationToken cancellationToken);

    Task<TaxRateResponse> GetTaxRateAsync(string municipalityName, DateOnly date, CancellationToken cancellationToken);
}
