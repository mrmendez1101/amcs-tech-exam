namespace Job.Marketplace.API.Features.Jobs.Delete;

public interface IDeleteJobQueries
{
    Task<bool> JobExistsAsync(Guid id, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}

public sealed class DeleteJobQueries(IDbConnectionFactory factory) : IDeleteJobQueries
{
    public async Task<bool> JobExistsAsync(Guid id, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        return await conn.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                "SELECT EXISTS(SELECT 1 FROM jobs WHERE id = @id)",
                new { id }, cancellationToken: ct));
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        await conn.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM jobs WHERE id = @id",
                new { id }, cancellationToken: ct));
    }
}
