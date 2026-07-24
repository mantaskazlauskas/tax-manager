using System.Net;
using System.Net.Http.Json;
using TaxManager.Api.IntegrationTests.Fixtures;
using TaxManager.Application.Dtos;
using TaxManager.Domain.Enums;

namespace TaxManager.Api.IntegrationTests;

[Collection(ApiTestCollection.Name)]
public class TaxRateApiTests(TaxManagerApiFactory factory)
{
    [Fact]
    public async Task GetTaxRate_MatchesRequirementsExampleScenario()
    {
        var client = factory.CreateClient();
        var municipality = $"Copenhagen-{Guid.NewGuid():N}";

        await CreateTaxRecordAsync(client, municipality, TaxPeriodType.Yearly, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31), 0.2m);
        await CreateTaxRecordAsync(client, municipality, TaxPeriodType.Monthly, new DateOnly(2024, 5, 1), new DateOnly(2024, 5, 31), 0.4m);
        await CreateTaxRecordAsync(client, municipality, TaxPeriodType.Daily, new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 1), 0.1m);
        await CreateTaxRecordAsync(client, municipality, TaxPeriodType.Daily, new DateOnly(2024, 12, 25), new DateOnly(2024, 12, 25), 0.1m);

        await AssertTaxRateAsync(client, municipality, new DateOnly(2024, 1, 1), 0.1m);
        await AssertTaxRateAsync(client, municipality, new DateOnly(2024, 3, 16), 0.2m);
        await AssertTaxRateAsync(client, municipality, new DateOnly(2024, 5, 2), 0.4m);
        await AssertTaxRateAsync(client, municipality, new DateOnly(2024, 7, 10), 0.2m);
    }

    [Fact]
    public async Task GetTaxRate_ReturnsNotFound_WhenMunicipalityDoesNotExist()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/municipalities/{Guid.NewGuid():N}/tax-rate?date=2024-01-01");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTaxRate_ReturnsNotFound_WhenNoRecordCoversTheDate()
    {
        var client = factory.CreateClient();
        var municipality = $"Aarhus-{Guid.NewGuid():N}";

        await CreateTaxRecordAsync(client, municipality, TaxPeriodType.Yearly, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31), 0.2m);

        var response = await client.GetAsync($"/api/municipalities/{municipality}/tax-rate?date=2025-01-01");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task CreateTaxRecordAsync(
        HttpClient client, string municipality, TaxPeriodType periodType, DateOnly start, DateOnly end, decimal rate)
    {
        var request = new CreateTaxRecordRequest(municipality, periodType, start, end, rate);
        var response = await client.PostAsJsonAsync("/api/tax-records", request, JsonDefaults.Options);
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
