using Microsoft.AspNetCore.Mvc;
using TaxManager.Application.Abstractions;
using TaxManager.Application.Dtos;

namespace TaxManager.Api.Controllers;

[ApiController]
[Route("api/tax-records")]
public class TaxRecordsController(ITaxService taxService) : ControllerBase
{
    /// <summary>Adds a new tax record for a municipality (created automatically if it doesn't exist yet).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaxRecordResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaxRecordResponse>> Create(CreateTaxRecordRequest request, CancellationToken cancellationToken)
    {
        var response = await taxService.AddTaxRecordAsync(request, cancellationToken);
        return Created($"/api/tax-records/{response.Id}", response);
    }

    /// <summary>Updates an existing tax record's period and rate. (Bonus requirement.)</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TaxRecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaxRecordResponse>> Update(int id, UpdateTaxRecordRequest request, CancellationToken cancellationToken)
    {
        var response = await taxService.UpdateTaxRecordAsync(id, request, cancellationToken);
        return Ok(response);
    }
}
