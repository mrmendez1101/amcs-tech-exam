namespace Job.Marketplace.API.Features.JobOffers.Accept;

public sealed class AcceptJobOfferHandler(IAcceptJobOfferQueries queries)
{
    public async Task HandleAsync(AcceptJobOfferCommand cmd, CancellationToken ct)
    {
        var snapshot = await queries.GetJobSnapshotAsync(cmd.JobId, ct);

        if (snapshot is null || snapshot.CustomerId != cmd.CustomerId)
            throw new KeyNotFoundException($"Job '{cmd.JobId}' not found.");

        if (snapshot.AcceptedOfferId is not null)
            throw new InvalidOperationException($"Job '{cmd.JobId}' already has an accepted offer.");

        if (!await queries.OfferBelongsToJobAsync(cmd.JobId, cmd.OfferId, ct))
            throw new KeyNotFoundException($"Offer '{cmd.OfferId}' not found for job '{cmd.JobId}'.");

        await queries.AcceptAsync(cmd.JobId, cmd.OfferId, ct);
    }
}
