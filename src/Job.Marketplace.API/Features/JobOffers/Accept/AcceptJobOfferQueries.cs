namespace Job.Marketplace.API.Features.JobOffers.Accept;

public sealed record AcceptJobOfferCommand(Guid JobId, Guid OfferId);

public interface IAcceptJobOfferQueries
{
    Task<bool> JobExistsAsync(Guid jobId, CancellationToken ct);
    Task<bool> OfferBelongsToJobAsync(Guid jobId, Guid offerId, CancellationToken ct);
    Task AcceptAsync(Guid jobId, Guid offerId, CancellationToken ct);
}

public sealed class AcceptJobOfferQueries(IDbConnectionFactory factory) : IAcceptJobOfferQueries
{
    public async Task<bool> JobExistsAsync(Guid jobId, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        return await conn.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                "SELECT EXISTS(SELECT 1 FROM jobs WHERE id = @jobId)",
                new { jobId }, cancellationToken: ct));
    }

    public async Task<bool> OfferBelongsToJobAsync(Guid jobId, Guid offerId, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        return await conn.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                "SELECT EXISTS(SELECT 1 FROM job_offers WHERE id = @offerId AND job_id = @jobId)",
                new { offerId, jobId }, cancellationToken: ct));
    }

    public async Task AcceptAsync(Guid jobId, Guid offerId, CancellationToken ct)
    {
        const string sql = "UPDATE jobs SET accepted_offer_id = @offerId WHERE id = @jobId";
        await using var conn = await factory.CreateAsync(ct);
        await conn.ExecuteAsync(new CommandDefinition(sql, new { jobId, offerId }, cancellationToken: ct));
    }
}
