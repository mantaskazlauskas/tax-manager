using MediatR;
using TaxManager.Application.Dtos;

namespace TaxManager.Application.Queries;

public record GetTaxRateQuery(string MunicipalityName, DateOnly Date) : IRequest<TaxRateResponse>;
