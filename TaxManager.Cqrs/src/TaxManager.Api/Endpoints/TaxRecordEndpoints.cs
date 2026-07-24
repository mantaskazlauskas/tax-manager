using MediatR;
using TaxManager.Application.Commands;
using TaxManager.Application.Dtos;

namespace TaxManager.Api.Endpoints;

public static class TaxRecordEndpoints
{
    public static IEndpointRouteBuilder MapTaxRecordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tax-records").WithTags("TaxRecords");

        // Adds a new tax record for a municipality (created automatically if it doesn't exist yet).
        group.MapPost("/", async (AddTaxRecordCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/tax-records/{response.Id}", response);
        })
        .Produces<TaxRecordResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        // Updates an existing tax record's period and rate. (Bonus requirement.)
        group.MapPut("/{id:guid}", async (Guid id, UpdateTaxRecordRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new UpdateTaxRecordCommand(id, request.PeriodType, request.StartDate, request.EndDate, request.Rate);
            var response = await sender.Send(command, cancellationToken);
            return Results.Ok(response);
        })
        .Produces<TaxRecordResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
