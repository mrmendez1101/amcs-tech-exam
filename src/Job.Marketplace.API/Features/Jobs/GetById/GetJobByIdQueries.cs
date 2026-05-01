namespace Job.Marketplace.API.Features.Jobs.GetById;

public interface IGetJobByIdQueries
{
    Task<JobDetail?> GetByIdAsync(Guid id, CancellationToken ct);
}

public sealed class GetJobByIdQueries(IDbConnectionFactory factory) : IGetJobByIdQueries
{
    private const string Sql = """
        SELECT id, customer_id, start_date, due_date, budget, description,
               accepted_offer_id, created_at
        FROM jobs WHERE id = @id
        """;

    public async Task<JobDetail?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<JobDetail>(
            new CommandDefinition(Sql, new { id }, cancellationToken: ct));
    }
}
