using Npgsql;

namespace Job.Marketplace.Infrastructure.Persistence;

public interface IDbConnectionFactory
{
    Task<NpgsqlConnection> CreateAsync(CancellationToken ct = default);
}
