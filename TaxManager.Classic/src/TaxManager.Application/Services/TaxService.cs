using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using TaxManager.Application.Abstractions;
using TaxManager.Application.Caching;
using TaxManager.Application.Dtos;
using TaxManager.Application.Options;
using TaxManager.Domain.Entities;
using TaxManager.Domain.Exceptions;
using TaxManager.Domain.Services;

namespace TaxManager.Application.Services;

public class TaxService(
    IMunicipalityRepository municipalityRepository,
    ITaxRecordRepository taxRecordRepository,
    IUnitOfWork unitOfWork,
    IDistributedCache cache,
    IOptions<CachingOptions> cachingOptions) : ITaxService
{
    public async Task<TaxRecordResponse> AddTaxRecordAsync(CreateTaxRecordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.MunicipalityName))
        {
            throw new ValidationException("Municipality name is required.");
        }

        TaxRecord.EnsureValidRange(request.PeriodType, request.StartDate, request.EndDate);

        var municipality = await municipalityRepository.GetByNameAsync(request.MunicipalityName, cancellationToken);
        if (municipality is null)
        {
            municipality = new Municipality(request.MunicipalityName);
            await municipalityRepository.AddAsync(municipality, cancellationToken);
        }

        var existingRecords = await taxRecordRepository.GetByMunicipalityIdAsync(municipality.Id, cancellationToken);
        EnsureNoOverlap(existingRecords, request.PeriodType, request.StartDate, request.EndDate, excludeId: null, municipality.Name);

        var taxRecord = new TaxRecord(municipality.Id, request.PeriodType, request.StartDate, request.EndDate, request.Rate);

        municipality.AddTaxRecord(taxRecord);

        await taxRecordRepository.AddAsync(taxRecord, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(TaxRecordCacheKeys.BuildKey(municipality.Name), cancellationToken);

        return ToResponse(taxRecord, municipality.Name);
    }

    public async Task<TaxRecordResponse> UpdateTaxRecordAsync(int taxRecordId, UpdateTaxRecordRequest request, CancellationToken cancellationToken)
    {
        TaxRecord.EnsureValidRange(request.PeriodType, request.StartDate, request.EndDate);

        var taxRecord = await taxRecordRepository.GetByIdAsync(taxRecordId, cancellationToken)
            ?? throw new TaxRecordNotFoundException(taxRecordId);

        var municipality = await municipalityRepository.GetByIdAsync(taxRecord.MunicipalityId, cancellationToken)
            ?? throw new MunicipalityNotFoundException(taxRecord.MunicipalityId.ToString());

        var existingRecords = await taxRecordRepository.GetByMunicipalityIdAsync(municipality.Id, cancellationToken);
        EnsureNoOverlap(existingRecords, request.PeriodType, request.StartDate, request.EndDate, excludeId: taxRecordId, municipality.Name);

        taxRecord.Update(request.PeriodType, request.StartDate, request.EndDate, request.Rate);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(TaxRecordCacheKeys.BuildKey(municipality.Name), cancellationToken);

        return ToResponse(taxRecord, municipality.Name);
    }

    public async Task<TaxRateResponse> GetTaxRateAsync(string municipalityName, DateOnly date, CancellationToken cancellationToken)
    {
        var municipality = await municipalityRepository.GetByNameAsync(municipalityName, cancellationToken)
            ?? throw new MunicipalityNotFoundException(municipalityName);

        var records = await GetCachedRecordsAsync(municipality, cancellationToken);
        var resolved = TaxRateResolver.Resolve(records, date)
            ?? throw new TaxRateNotFoundException(municipality.Name, date);

        return new TaxRateResponse(municipality.Name, date, resolved.Rate, resolved.PeriodType);
    }

    private async Task<IReadOnlyList<TaxRecord>> GetCachedRecordsAsync(Municipality municipality, CancellationToken cancellationToken)
    {
        var key = TaxRecordCacheKeys.BuildKey(municipality.Name);
        var cached = await cache.GetStringAsync(key, cancellationToken);
        if (cached is not null)
        {
            var cachedRecords = JsonSerializer.Deserialize<List<CachedTaxRecord>>(cached) ?? [];
            return cachedRecords
                .Select(r => new TaxRecord(r.MunicipalityId, r.PeriodType, r.StartDate, r.EndDate, r.Rate))
                .ToList();
        }

        var records = await taxRecordRepository.GetByMunicipalityIdAsync(municipality.Id, cancellationToken);
        var toCache = records.Select(r => new CachedTaxRecord(r.MunicipalityId, r.PeriodType, r.StartDate, r.EndDate, r.Rate));
        await cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(toCache),
            new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(cachingOptions.Value.TaxRatesSlidingExpirationMinutes)
            },
            cancellationToken);

        return records;
    }

    /// <summary>
    /// Assumption: overlapping ranges of the *same* period type for the *same* municipality are
    /// rejected, since there would be no defined tie-breaker between them. Overlaps across
    /// different period types are fine - that's what TaxRateResolver's priority order is for.
    /// </summary>
    private static void EnsureNoOverlap(
        IReadOnlyList<TaxRecord> existingRecords,
        Domain.Enums.TaxPeriodType periodType,
        DateOnly startDate,
        DateOnly endDate,
        int? excludeId,
        string municipalityName)
    {
        var hasOverlap = existingRecords.Any(record =>
            record.Id != excludeId &&
            record.PeriodType == periodType &&
            record.OverlapsWith(startDate, endDate));

        if (hasOverlap)
        {
            throw new OverlappingTaxPeriodException(municipalityName);
        }
    }

    private static TaxRecordResponse ToResponse(TaxRecord taxRecord, string municipalityName) =>
        new(taxRecord.Id, municipalityName, taxRecord.PeriodType, taxRecord.StartDate, taxRecord.EndDate, taxRecord.Rate);
}
