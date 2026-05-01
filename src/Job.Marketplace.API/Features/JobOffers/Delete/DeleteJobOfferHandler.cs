namespace Job.Marketplace.API.Features.JobOffers.Delete;

public sealed class DeleteJobOfferHandler(IDeleteJobOfferQueries queries)
{
    public async Task HandleAsync(Guid jobId, Guid offerId, CancellationToken ct)
    {
        if (!await queries.OfferExistsAsync(jobId, offerId, ct))
            throw new KeyNotFoundException($"Offer '{offerId}' not found for job '{jobId}'.");

        await queries.DeleteAsync(offerId, ct);
    }
}
