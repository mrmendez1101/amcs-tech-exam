namespace Job.Marketplace.API.Features.JobOffers.Delete;

public interface IDeleteJobOfferQueries
{
    Task<string?> GetJobStatusForOfferAsync(Guid jobId, Guid offerId, CancellationToken ct);
    Task DeleteAsync(Guid offerId, CancellationToken ct);
}

public sealed class DeleteJobOfferQueries(IDbConnectionFactory factory) : IDeleteJobOfferQueries
{
    public async Task<string?> GetJobStatusForOfferAsync(Guid jobId, Guid offerId, CancellationToken ct)
    {
        const string sql = """
            SELECT j.status
            FROM jobs j
            INNER JOIN job_offers o ON o.job_id = j.id
            WHERE j.id = @jobId AND o.id = @offerId
            """;
        await using var conn = await factory.CreateAsync(ct);
        return await conn.ExecuteScalarAsync<string?>(
            new CommandDefinition(sql, new { jobId, offerId }, cancellationToken: ct));
    }

    public async Task DeleteAsync(Guid offerId, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        await conn.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM job_offers WHERE id = @offerId",
                new { offerId }, cancellationToken: ct));
    }
}
