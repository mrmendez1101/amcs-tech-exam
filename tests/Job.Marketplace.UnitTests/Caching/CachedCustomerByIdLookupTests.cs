using FluentAssertions;
using Job.Marketplace.API.Features.Customers.GetById;
using Job.Marketplace.Infrastructure.Caching;
using NSubstitute;

namespace Job.Marketplace.UnitTests.Caching;

public class CachedCustomerByIdLookupTests
{
    private static CustomerDetail MakeDto(Guid id) =>
        new(id, "John", "Smith", DateTimeOffset.UtcNow);

    private static CachedCustomerByIdLookup BuildSut(
        IGetCustomerByIdQueries inner,
        LruCache<Guid, CustomerDetail> cache,
        CacheMetrics metrics) => new(inner, cache, metrics);

    [Fact]
    public async Task First_call_misses_cache_and_hits_inner()
    {
        var inner = Substitute.For<IGetCustomerByIdQueries>();
        var dto = MakeDto(Guid.NewGuid());
        inner.GetByIdAsync(dto.Id, Arg.Any<CancellationToken>()).Returns(dto);
        var cache = new LruCache<Guid, CustomerDetail>(10);
        var metrics = new CacheMetrics();

        var result = await BuildSut(inner, cache, metrics).GetByIdAsync(dto.Id, default);

        result.Should().Be(dto);
        metrics.Misses.Should().Be(1);
        metrics.Hits.Should().Be(0);
    }

    [Fact]
    public async Task Second_call_hits_cache_and_skips_inner()
    {
        var inner = Substitute.For<IGetCustomerByIdQueries>();
        var dto = MakeDto(Guid.NewGuid());
        inner.GetByIdAsync(dto.Id, Arg.Any<CancellationToken>()).Returns(dto);
        var cache = new LruCache<Guid, CustomerDetail>(10);
        var metrics = new CacheMetrics();
        var sut = BuildSut(inner, cache, metrics);

        await sut.GetByIdAsync(dto.Id, default);
        var result = await sut.GetByIdAsync(dto.Id, default);

        result.Should().Be(dto);
        await inner.Received(1).GetByIdAsync(dto.Id, Arg.Any<CancellationToken>());
        metrics.Hits.Should().Be(1);
        metrics.Misses.Should().Be(1);
    }

    [Fact]
    public async Task Null_result_is_not_cached()
    {
        var inner = Substitute.For<IGetCustomerByIdQueries>();
        var missingId = Guid.NewGuid();
        inner.GetByIdAsync(missingId, Arg.Any<CancellationToken>())
             .Returns((CustomerDetail?)null);
        var cache = new LruCache<Guid, CustomerDetail>(10);
        var metrics = new CacheMetrics();
        var sut = BuildSut(inner, cache, metrics);

        await sut.GetByIdAsync(missingId, default);
        await sut.GetByIdAsync(missingId, default);

        await inner.Received(2).GetByIdAsync(missingId, Arg.Any<CancellationToken>());
        metrics.Misses.Should().Be(2);
        metrics.Hits.Should().Be(0);
    }
}
