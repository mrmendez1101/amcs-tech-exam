using Job.Marketplace.Infrastructure.Caching;

namespace Job.Marketplace.API.Features.Customers.GetById;

public sealed class CachedCustomerByIdLookup(
    IGetCustomerByIdQueries inner,
    LruCache<Guid, CustomerDetail> cache,
    CacheMetrics metrics) : IGetCustomerByIdQueries
{
    public async Task<CustomerDetail?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        if (cache.TryGet(id, out var hit))
        {
            metrics.RecordHit();
            return hit;
        }

        metrics.RecordMiss();
        var fresh = await inner.GetByIdAsync(id, ct);
        if (fresh is not null) cache.Set(id, fresh);
        return fresh;
    }
}
