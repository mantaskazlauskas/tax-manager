using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using TaxManager.Application.Caching;
using TaxManager.Application.Options;

namespace TaxManager.Application.Behaviors;

/// <summary>
/// Caches successful results of <see cref="ICacheableQuery{TResponse}"/> requests. Only a
/// successful <c>next()</c> is cached - if the handler throws (e.g. a not-found), nothing is
/// stored, so a "not found" is never itself cached. Placed after <see cref="ValidationBehavior{TRequest,TResponse}"/>
/// in the pipeline so invalid requests never reach the cache.
/// </summary>
public class CachingBehavior<TRequest, TResponse>(IDistributedCache cache, IOptions<CachingOptions> cachingOptions)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var generationKey = TaxRecordCacheKeys.GenerationKey(request.CacheScope);
        var generation = await cache.GetStringAsync(generationKey, cancellationToken);
        if (generation is null)
        {
            generation = Guid.NewGuid().ToString("N");
            await cache.SetStringAsync(generationKey, generation, cancellationToken);
        }

        var dataKey = TaxRecordCacheKeys.DataKey(request.CacheScope, request.CacheKeySuffix, generation);

        var cached = await cache.GetStringAsync(dataKey, cancellationToken);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<TResponse>(cached)!;
        }

        var response = await next();

        await cache.SetStringAsync(
            dataKey,
            JsonSerializer.Serialize(response),
            new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(cachingOptions.Value.TaxRatesSlidingExpirationMinutes)
            },
            cancellationToken);

        return response;
    }
}
