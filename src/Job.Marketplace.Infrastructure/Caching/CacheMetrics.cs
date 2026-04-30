namespace Job.Marketplace.Infrastructure.Caching;

public sealed class CacheMetrics
{
    private long _hits;
    private long _misses;

    public long Hits => Interlocked.Read(ref _hits);
    public long Misses => Interlocked.Read(ref _misses);
    public double HitRatio
    {
        get
        {
            var h = Hits;
            var total = h + Misses;
            return total == 0 ? 0 : (double)h / total;
        }
    }

    public void RecordHit() => Interlocked.Increment(ref _hits);
    public void RecordMiss() => Interlocked.Increment(ref _misses);
}
