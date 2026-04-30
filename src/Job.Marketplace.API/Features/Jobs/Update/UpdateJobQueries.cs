namespace Job.Marketplace.API.Features.Jobs.Update;

public interface IUpdateJobQueries
{
    Task<string?> GetJobStatusAsync(Guid id, CancellationToken ct);
    Task UpdateAsync(UpdateJobCommand cmd, CancellationToken ct);
}

public sealed class UpdateJobQueries(IDbConnectionFactory factory) : IUpdateJobQueries
{
    public async Task<string?> GetJobStatusAsync(Guid id, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        return await conn.ExecuteScalarAsync<string?>(
            new CommandDefinition("SELECT status FROM jobs WHERE id = @id", new { id }, cancellationToken: ct));
    }

    public async Task UpdateAsync(UpdateJobCommand cmd, CancellationToken ct)
    {
        const string sql = """
            UPDATE jobs
            SET start_date = @StartDate, due_date = @DueDate, budget = @Budget, description = @Description
            WHERE id = @Id
            """;
        await using var conn = await factory.CreateAsync(ct);
        await conn.ExecuteAsync(new CommandDefinition(sql, cmd, cancellationToken: ct));
    }
}
