namespace Job.Marketplace.API.Features.JobOffers.GetByJobId;

public interface IGetJobOffersByJobIdQueries
{
    Task<bool> JobExistsAsync(Guid jobId, CancellationToken ct);
    Task<IReadOnlyList<JobOfferSummary>> GetByJobIdAsync(Guid jobId, CancellationToken ct);
}

public sealed class GetJobOffersByJobIdQueries(IDbConnectionFactory factory) : IGetJobOffersByJobIdQueries
{
    public async Task<bool> JobExistsAsync(Guid jobId, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        return await conn.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                "SELECT EXISTS(SELECT 1 FROM jobs WHERE id = @jobId)",
                new { jobId }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<JobOfferSummary>> GetByJobIdAsync(Guid jobId, CancellationToken ct)
    {
        const string sql = """
            SELECT id, contractor_id, price, created_at
            FROM job_offers
            WHERE job_id = @jobId
            ORDER BY created_at
            """;
        await using var conn = await factory.CreateAsync(ct);
        return (await conn.QueryAsync<JobOfferSummary>(
            new CommandDefinition(sql, new { jobId }, cancellationToken: ct))).ToList();
    }
}
