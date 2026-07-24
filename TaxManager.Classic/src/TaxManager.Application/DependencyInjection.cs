using Microsoft.Extensions.DependencyInjection;
using TaxManager.Application.Abstractions;
using TaxManager.Application.Services;

namespace TaxManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) =>
        services.AddScoped<ITaxService, TaxService>();
}
