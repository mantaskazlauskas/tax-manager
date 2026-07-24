using MediatR;
using TaxManager.Application.Dtos;
using TaxManager.Application.Queries;

namespace TaxManager.Api.Endpoints;

public static class TaxRateEndpoints
{
    public static IEndpointRouteBuilder MapTaxRateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/municipalities").WithTags("TaxRates");

        // Resolves the applicable tax rate for a municipality on a given date.
        group.MapGet("/{municipalityName}/tax-rate", async (string municipalityName, DateOnly date, ISender sender, CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(new GetTaxRateQuery(municipalityName, date), cancellationToken);
            return Results.Ok(response);
        })
        .Produces<TaxRateResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
