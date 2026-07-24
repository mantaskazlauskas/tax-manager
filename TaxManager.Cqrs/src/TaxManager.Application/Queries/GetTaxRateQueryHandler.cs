using MediatR;
using TaxManager.Application.Abstractions;
using TaxManager.Application.Dtos;
using TaxManager.Domain.Exceptions;
using TaxManager.Domain.Services;

namespace TaxManager.Application.Queries;

public class GetTaxRateQueryHandler(
    IMunicipalityRepository municipalityRepository,
    ITaxRecordRepository taxRecordRepository) : IRequestHandler<GetTaxRateQuery, TaxRateResponse>
{
    public async Task<TaxRateResponse> Handle(GetTaxRateQuery request, CancellationToken cancellationToken)
    {
        var municipality = await municipalityRepository.GetByNameAsync(request.MunicipalityName, cancellationToken)
            ?? throw new MunicipalityNotFoundException(request.MunicipalityName);

        var records = await taxRecordRepository.GetByMunicipalityIdAsync(municipality.Id, cancellationToken);
        var resolved = TaxRateResolver.Resolve(records, request.Date)
            ?? throw new TaxRateNotFoundException(municipality.Name, request.Date);

        return new TaxRateResponse(municipality.Name, request.Date, resolved.Rate, resolved.PeriodType);
    }
}
