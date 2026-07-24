using FluentValidation;
using MediatR;
using ValidationException = TaxManager.Domain.Exceptions.ValidationException;

namespace TaxManager.Application.Behaviors;

/// <summary>
/// Runs every registered FluentValidation validator for the request before it reaches its
/// handler, so handlers can assume they're only ever given valid input. Failures surface as a
/// domain <see cref="ValidationException"/>, which the API's global exception handler maps to 400.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(request, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(string.Join(" ", failures.Select(f => f.ErrorMessage)));
        }

        return await next();
    }
}
