using System.Net;
using System.Net.Http.Json;
using TaxManager.Api.IntegrationTests.Fixtures;
using TaxManager.Application.Dtos;
using TaxManager.Domain.Enums;

namespace TaxManager.Api.IntegrationTests;

[Collection(ApiTestCollection.Name)]
public class TaxRecordsApiTests(TaxManagerApiFactory factory)
{
    [Fact]
    public async Task Create_ReturnsCreated_AndPersistsTheRecord()
    {
        var client = factory.CreateClient();
        var municipality = $"Odense-{Guid.NewGuid():N}";
        var request = new CreateTaxRecordRequest(municipality, TaxPeriodType.Yearly, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31), 0.25m);

        var response = await client.PostAsJsonAsync("/api/tax-records", request, JsonDefaults.Options);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TaxRecordResponse>(JsonDefaults.Options);
        Assert.NotNull(created);
        Assert.Equal(0.25m, created.Rate);
        Assert.NotEqual(0, created.Id);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenSamePeriodTypeOverlapsAnExistingRecord()
    {
        var client = factory.CreateClient();
        var municipality = $"Aalborg-{Guid.NewGuid():N}";

        var first = new CreateTaxRecordRequest(municipality, TaxPeriodType.Monthly, new DateOnly(2024, 5, 1), new DateOnly(2024, 5, 31), 0.4m);
        (await client.PostAsJsonAsync("/api/tax-records", first, JsonDefaults.Options)).EnsureSuccessStatusCode();

        var overlapping = new CreateTaxRecordRequest(municipality, TaxPeriodType.Monthly, new DateOnly(2024, 5, 15), new DateOnly(2024, 6, 14), 0.5m);
        var response = await client.PostAsJsonAsync("/api/tax-records", overlapping, JsonDefaults.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_Allows_DifferentPeriodTypes_ToOverlapTheSameDateRange()
    {
        var client = factory.CreateClient();
        var municipality = $"Esbjerg-{Guid.NewGuid():N}";

        var yearly = new CreateTaxRecordRequest(municipality, TaxPeriodType.Yearly, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31), 0.2m);
        var daily = new CreateTaxRecordRequest(municipality, TaxPeriodType.Daily, new DateOnly(2024, 6, 1), new DateOnly(2024, 6, 1), 0.1m);

        var yearlyResponse = await client.PostAsJsonAsync("/api/tax-records", yearly, JsonDefaults.Options);
        var dailyResponse = await client.PostAsJsonAsync("/api/tax-records", daily, JsonDefaults.Options);

        Assert.Equal(HttpStatusCode.Created, yearlyResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, dailyResponse.StatusCode);
    }

    [Fact]
    public async Task Update_ChangesTheRateUsedByFutureQueries()
    {
        var client = factory.CreateClient();
        var municipality = $"Randers-{Guid.NewGuid():N}";

        var created = await (await client.PostAsJsonAsync(
            "/api/tax-records",
            new CreateTaxRecordRequest(municipality, TaxPeriodType.Yearly, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31), 0.2m),
            JsonDefaults.Options))
            .Content.ReadFromJsonAsync<TaxRecordResponse>(JsonDefaults.Options);
        Assert.NotNull(created);

        var update = new UpdateTaxRecordRequest(TaxPeriodType.Yearly, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31), 0.35m);
        var updateResponse = await client.PutAsJsonAsync($"/api/tax-records/{created.Id}", update, JsonDefaults.Options);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TaxRecordResponse>(JsonDefaults.Options);
        Assert.NotNull(updated);
        Assert.Equal(0.35m, updated.Rate);

        var rateResponse = await client.GetFromJsonAsync<TaxRateResponse>(
            $"/api/municipalities/{municipality}/tax-rate?date=2024-06-01", JsonDefaults.Options);
        Assert.NotNull(rateResponse);
        Assert.Equal(0.35m, rateResponse.Rate);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenTaxRecordDoesNotExist()
    {
        var client = factory.CreateClient();
        var update = new UpdateTaxRecordRequest(TaxPeriodType.Yearly, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31), 0.35m);

        var response = await client.PutAsJsonAsync($"/api/tax-records/{int.MaxValue}", update, JsonDefaults.Options);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
