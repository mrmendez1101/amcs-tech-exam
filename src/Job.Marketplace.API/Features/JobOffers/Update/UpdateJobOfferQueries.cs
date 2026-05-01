namespace Job.Marketplace.API.Features.JobOffers.Update;

public interface IUpdateJobOfferQueries
{
    Task<bool> OfferExistsAsync(Guid jobId, Guid offerId, CancellationToken ct);
    Task UpdateAsync(UpdateJobOfferCommand cmd, CancellationToken ct);
}

public sealed class UpdateJobOfferQueries(IDbConnectionFactory factory) : IUpdateJobOfferQueries
{
    public async Task<bool> OfferExistsAsync(Guid jobId, Guid offerId, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        return await conn.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                "SELECT EXISTS(SELECT 1 FROM job_offers WHERE id = @offerId AND job_id = @jobId)",
                new { offerId, jobId }, cancellationToken: ct));
    }

    public async Task UpdateAsync(UpdateJobOfferCommand cmd, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        await conn.ExecuteAsync(
            new CommandDefinition(
                "UPDATE job_offers SET price = @Price WHERE id = @OfferId",
                cmd, cancellationToken: ct));
    }
}
