namespace Job.Marketplace.API.Features.Jobs.Delete;

public interface IDeleteJobQueries
{
    Task<string?> GetJobStatusAsync(Guid id, CancellationToken ct);
    Task SoftDeleteAsync(Guid id, CancellationToken ct);
}

public sealed class DeleteJobQueries(IDbConnectionFactory factory) : IDeleteJobQueries
{
    public async Task<string?> GetJobStatusAsync(Guid id, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        return await conn.ExecuteScalarAsync<string?>(
            new CommandDefinition("SELECT status FROM jobs WHERE id = @id", new { id }, cancellationToken: ct));
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        await conn.ExecuteAsync(
            new CommandDefinition(
                "UPDATE jobs SET status = 'Cancelled' WHERE id = @id",
                new { id }, cancellationToken: ct));
    }
}
