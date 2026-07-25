using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaxManager.Domain.Exceptions;

namespace TaxManager.Api.Middleware;

/// <summary>
/// Catches every exception that reaches the end of the pipeline so nothing unmanaged ever leaks to
/// the caller. Known domain exceptions map to a precise 4xx ProblemDetails response; anything else
/// is logged in full server-side and reported to the caller as a generic 500 with no internal detail.
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = MapException(exception);
        var isUnexpected = statusCode == StatusCodes.Status500InternalServerError;

        if (isUnexpected)
        {
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning("{Title} processing {Method} {Path}: {Message}", title, httpContext.Request.Method, httpContext.Request.Path, exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = isUnexpected ? "An unexpected error occurred. Please try again later." : exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        MunicipalityNotFoundException or TaxRateNotFoundException or TaxRecordNotFoundException
            => (StatusCodes.Status404NotFound, "Not Found"),
        OverlappingTaxPeriodException or InvalidTaxPeriodRangeException or ValidationException or ArgumentException
            => (StatusCodes.Status400BadRequest, "Bad Request"),
        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
    };
}
