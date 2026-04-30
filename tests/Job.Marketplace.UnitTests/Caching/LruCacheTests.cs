using FluentAssertions;
using Job.Marketplace.Infrastructure.Caching;

namespace Job.Marketplace.UnitTests.Caching;

public class LruCacheTests
{
    [Fact]
    public void TryGet_returns_false_when_key_missing()
    {
        var sut = new LruCache<int, string>(capacity: 3);
        sut.TryGet(1, out _).Should().BeFalse();
    }

    [Fact]
    public void Set_then_TryGet_returns_value()
    {
        var sut = new LruCache<int, string>(capacity: 3);
        sut.Set(1, "one");
        sut.TryGet(1, out var value).Should().BeTrue();
        value.Should().Be("one");
    }

    [Fact]
    public void Evicts_least_recently_used_when_at_capacity()
    {
        var sut = new LruCache<int, string>(capacity: 2);
        sut.Set(1, "one");
        sut.Set(2, "two");
        sut.Set(3, "three");

        sut.TryGet(1, out _).Should().BeFalse();
        sut.TryGet(2, out _).Should().BeTrue();
        sut.TryGet(3, out _).Should().BeTrue();
    }

    [Fact]
    public void TryGet_promotes_key_to_most_recently_used()
    {
        var sut = new LruCache<int, string>(capacity: 2);
        sut.Set(1, "one");
        sut.Set(2, "two");
        sut.TryGet(1, out _);
        sut.Set(3, "three");

        sut.TryGet(1, out _).Should().BeTrue();
        sut.TryGet(2, out _).Should().BeFalse();
        sut.TryGet(3, out _).Should().BeTrue();
    }

    [Fact]
    public void Set_existing_key_updates_value_and_moves_to_front()
    {
        var sut = new LruCache<int, string>(capacity: 2);
        sut.Set(1, "one");
        sut.Set(2, "two");
        sut.Set(1, "ONE");
        sut.Set(3, "three");

        sut.TryGet(1, out var v1).Should().BeTrue();
        v1.Should().Be("ONE");
        sut.TryGet(2, out _).Should().BeFalse();
    }

    [Fact]
    public void Remove_returns_true_when_key_existed_and_clears_it()
    {
        var sut = new LruCache<int, string>(capacity: 3);
        sut.Set(1, "one");
        sut.Remove(1).Should().BeTrue();
        sut.TryGet(1, out _).Should().BeFalse();
        sut.Remove(1).Should().BeFalse();
    }

    [Fact]
    public async Task Concurrent_access_does_not_corrupt_state()
    {
        var sut = new LruCache<int, int>(capacity: 100);
        var tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                sut.Set(i % 200, i);
                sut.TryGet(i % 200, out _);
            }
        }));

        await Task.WhenAll(tasks);

        sut.Count.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void Constructor_rejects_zero_or_negative_capacity()
    {
        Action act = () => _ = new LruCache<int, int>(0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
