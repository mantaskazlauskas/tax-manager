using Microsoft.AspNetCore.Mvc;
using TaxManager.Application.Abstractions;
using TaxManager.Application.Dtos;

namespace TaxManager.Api.Controllers;

[ApiController]
[Route("api/municipalities")]
public class TaxRatesController(ITaxService taxService) : ControllerBase
{
    /// <summary>Resolves the applicable tax rate for a municipality on a given date.</summary>
    [HttpGet("{municipalityName}/tax-rate")]
    [ProducesResponseType(typeof(TaxRateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaxRateResponse>> GetTaxRate(
        string municipalityName,
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken)
    {
        var response = await taxService.GetTaxRateAsync(municipalityName, date, cancellationToken);
        return Ok(response);
    }
}
