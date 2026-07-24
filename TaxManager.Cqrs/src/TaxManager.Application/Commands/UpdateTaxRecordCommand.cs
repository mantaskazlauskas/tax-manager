using MediatR;
using TaxManager.Application.Dtos;
using TaxManager.Domain.Enums;

namespace TaxManager.Application.Commands;

public record UpdateTaxRecordCommand(
    Guid TaxRecordId,
    TaxPeriodType PeriodType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Rate) : IRequest<TaxRecordResponse>;
