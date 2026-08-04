using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaxManager.Api.IntegrationTests.Fixtures;
using TaxManager.Application.Dtos;
using TaxManager.Domain.Enums;
using TaxManager.Infrastructure.Persistence;

namespace TaxManager.Api.IntegrationTests;

/// <summary>
/// Proves the tax-rate cache is actually in the read path - not just that the API returns correct
/// values - by mutating the database directly (bypassing the API's write/invalidation path) and
/// checking that a cached read keeps serving the old value until a real write invalidates it.
/// </summary>
[Collection(ApiTestCollection.Name)]
public class TaxRateCachingTests(TaxManagerApiFactory factory)
{
    [Fact]
    public async Task GetTaxRate_ServesStaleCachedValue_UntilTheRecordIsUpdatedThroughTheApi()
    {
        var client = factory.CreateClient();
        var municipality = $"Silkeborg-{Guid.NewGuid():N}";
        var date = new DateOnly(2024, 1, 1);

        var recordId = await CreateTaxRecordAsync(client, municipality, TaxPeriodType.Yearly, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31), 0.2m);

        await AssertTaxRateAsync(client, municipality, date, 0.2m); // primes the cache

        await MutateRateDirectlyInDatabaseAsync(recordId, 0.9m);

        // If reads went straight to the database this would already be 0.9m - it still being
        // 0.2m proves the response is coming from the cache.
        await AssertTaxRateAsync(client, municipality, date, 0.2m);

        await UpdateTaxRecordAsync(client, recordId, TaxPeriodType.Yearly, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31), 0.5m);

        // The real update endpoint invalidates the cache, so the next read reflects it.
        await AssertTaxRateAsync(client, municipality, date, 0.5m);
    }

    [Fact]
    public async Task GetTaxRate_InvalidatesEveryCachedDateForAMunicipality_WhenAnyRecordIsAdded()
    {
        var client = factory.CreateClient();
        var municipality = $"Kolding-{Guid.NewGuid():N}";
        var yearlyDate = new DateOnly(2024, 7, 10);

        var yearlyRecordId = await CreateTaxRecordAsync(client, municipality, TaxPeriodType.Yearly, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31), 0.2m);

        await AssertTaxRateAsync(client, municipality, yearlyDate, 0.2m); // primes the cache for yearlyDate

        await MutateRateDirectlyInDatabaseAsync(yearlyRecordId, 0.6m);

        // Adding an unrelated Daily record (a different date, no overlap) still bumps the
        // municipality's cache generation, so the previously-cached yearlyDate entry is
        // invalidated too - proving invalidation is scoped to the whole municipality, not just
        // the single key a write happens to touch.
        await CreateTaxRecordAsync(client, municipality, TaxPeriodType.Daily, new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 1), 0.1m);

        await AssertTaxRateAsync(client, municipality, yearlyDate, 0.6m);
    }

    private async Task MutateRateDirectlyInDatabaseAsync(int recordId, decimal newRate)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaxManagerDbContext>();
        var record = await dbContext.TaxRecords.SingleAsync(r => r.Id == recordId);
        record.Update(record.PeriodType, record.StartDate, record.EndDate, newRate);
        await dbContext.SaveChangesAsync();
    }

    private static async Task<int> CreateTaxRecordAsync(
        HttpClient client, string municipality, TaxPeriodType periodType, DateOnly start, DateOnly end, decimal rate)
    {
        var request = new CreateTaxRecordRequest(municipality, periodType, start, end, rate);
        var response = await client.PostAsJsonAsync("/api/tax-records", request, JsonDefaults.Options);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<TaxRecordResponse>(JsonDefaults.Options);
        Assert.NotNull(created);
        return created.Id;
    }

    private static async Task UpdateTaxRecordAsync(
        HttpClient client, int recordId, TaxPeriodType periodType, DateOnly start, DateOnly end, decimal rate)
    {
        var request = new UpdateTaxRecordRequest(periodType, start, end, rate);
        var response = await client.PutAsJsonAsync($"/api/tax-records/{recordId}", request, JsonDefaults.Options);
        response.EnsureSuccessStatusCode();
    }

    private static async Task AssertTaxRateAsync(HttpClient client, string municipality, DateOnly date, decimal expectedRate)
    {
        var response = await client.GetAsync($"/api/municipalities/{municipality}/tax-rate?date={date:yyyy-MM-dd}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TaxRateResponse>(JsonDefaults.Options);

        Assert.NotNull(result);
        Assert.Equal(expectedRate, result.Rate);
    }
}
