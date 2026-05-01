namespace Job.Marketplace.API.Features.JobOffers.Accept;

public sealed class AcceptJobOfferHandler(IAcceptJobOfferQueries queries)
{
    public async Task HandleAsync(AcceptJobOfferCommand cmd, CancellationToken ct)
    {
        if (!await queries.JobExistsAsync(cmd.JobId, ct))
            throw new KeyNotFoundException($"Job '{cmd.JobId}' not found.");

        if (!await queries.OfferBelongsToJobAsync(cmd.JobId, cmd.OfferId, ct))
            throw new KeyNotFoundException($"Offer '{cmd.OfferId}' not found for job '{cmd.JobId}'.");

        await queries.AcceptAsync(cmd.JobId, cmd.OfferId, ct);
    }
}
