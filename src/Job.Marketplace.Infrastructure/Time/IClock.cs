namespace Job.Marketplace.Infrastructure.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
