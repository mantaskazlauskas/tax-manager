using MediatR;

namespace TaxManager.Application.Caching;

/// <summary>
/// Marks a query as cacheable by <see cref="Behaviors.CachingBehavior{TRequest,TResponse}"/>. Only
/// queries implement this - commands go through the pipeline unaffected.
///
/// <see cref="CacheScope"/> is the unit invalidation acts on (here, the municipality name) - the
/// behavior tags every key derived from a request with a per-scope generation token so a write can
/// invalidate every cached query for that scope in O(1), without enumerating individual keys
/// (IDistributedCache has no prefix-eviction). <see cref="CacheKeySuffix"/> is whatever else
/// distinguishes cache entries within that scope (here, the queried date).
/// </summary>
public interface ICacheableQuery<TResponse> : IRequest<TResponse>
{
    string CacheScope { get; }
    string CacheKeySuffix { get; }
}
