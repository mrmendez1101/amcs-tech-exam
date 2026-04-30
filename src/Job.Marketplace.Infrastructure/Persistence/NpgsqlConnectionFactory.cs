using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Job.Marketplace.Infrastructure.Persistence;

public sealed class NpgsqlConnectionFactory(IConfiguration config) : IDbConnectionFactory
{
    private readonly string _connString =
        config.GetConnectionString("Marketplace")
        ?? throw new InvalidOperationException("ConnectionStrings:Marketplace is not configured.");

    public async Task<NpgsqlConnection> CreateAsync(CancellationToken ct = default)
    {
        var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync(ct);
        return conn;
    }
}
