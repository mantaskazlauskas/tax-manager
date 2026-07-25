using MediatR;
using TaxManager.Application.Abstractions;
using TaxManager.Application.Common;
using TaxManager.Application.Dtos;
using TaxManager.Domain.Entities;
using TaxManager.Domain.Exceptions;

namespace TaxManager.Application.Commands;

public class UpdateTaxRecordCommandHandler(
    IMunicipalityRepository municipalityRepository,
    ITaxRecordRepository taxRecordRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateTaxRecordCommand, TaxRecordResponse>
{
    public async Task<TaxRecordResponse> Handle(UpdateTaxRecordCommand request, CancellationToken cancellationToken)
    {
        TaxRecord.EnsureValidRange(request.PeriodType, request.StartDate, request.EndDate);

        var taxRecord = await taxRecordRepository.GetByIdAsync(request.TaxRecordId, cancellationToken)
            ?? throw new TaxRecordNotFoundException(request.TaxRecordId);

        var municipality = await municipalityRepository.GetByIdAsync(taxRecord.MunicipalityId, cancellationToken)
            ?? throw new MunicipalityNotFoundException(taxRecord.MunicipalityId.ToString());

        var existingRecords = await taxRecordRepository.GetByMunicipalityIdAsync(municipality.Id, cancellationToken);
        OverlapGuard.EnsureNoOverlap(existingRecords, request.PeriodType, request.StartDate, request.EndDate, request.TaxRecordId, municipality.Name);

        taxRecord.Update(request.PeriodType, request.StartDate, request.EndDate, request.Rate);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TaxRecordResponse(taxRecord.Id, municipality.Name, taxRecord.PeriodType, taxRecord.StartDate, taxRecord.EndDate, taxRecord.Rate);
    }
}
