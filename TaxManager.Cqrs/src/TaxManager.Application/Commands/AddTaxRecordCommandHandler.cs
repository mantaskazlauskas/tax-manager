using MediatR;
using TaxManager.Application.Abstractions;
using TaxManager.Application.Common;
using TaxManager.Application.Dtos;
using TaxManager.Domain.Entities;

namespace TaxManager.Application.Commands;

public class AddTaxRecordCommandHandler(
    IMunicipalityRepository municipalityRepository,
    ITaxRecordRepository taxRecordRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AddTaxRecordCommand, TaxRecordResponse>
{
    public async Task<TaxRecordResponse> Handle(AddTaxRecordCommand request, CancellationToken cancellationToken)
    {
        TaxRecord.EnsureValidRange(request.PeriodType, request.StartDate, request.EndDate);

        var municipality = await municipalityRepository.GetByNameAsync(request.MunicipalityName, cancellationToken);
        if (municipality is null)
        {
            municipality = new Municipality(request.MunicipalityName);
            await municipalityRepository.AddAsync(municipality, cancellationToken);
        }

        var existingRecords = await taxRecordRepository.GetByMunicipalityIdAsync(municipality.Id, cancellationToken);
        OverlapGuard.EnsureNoOverlap(existingRecords, request.PeriodType, request.StartDate, request.EndDate, excludeId: null, municipality.Name);

        var taxRecord = new TaxRecord(municipality.Id, request.PeriodType, request.StartDate, request.EndDate, request.Rate);

        municipality.AddTaxRecord(taxRecord);

        await taxRecordRepository.AddAsync(taxRecord, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TaxRecordResponse(taxRecord.Id, municipality.Name, taxRecord.PeriodType, taxRecord.StartDate, taxRecord.EndDate, taxRecord.Rate);
    }
}
