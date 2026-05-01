namespace Job.Marketplace.API.Features.JobOffers.Delete;

public sealed class DeleteJobOfferHandler(IDeleteJobOfferQueries queries)
{
    public async Task HandleAsync(Guid jobId, Guid offerId, CancellationToken ct)
    {
        var status = await queries.GetJobStatusForOfferAsync(jobId, offerId, ct);
        if (status is null)
            throw new KeyNotFoundException($"Offer '{offerId}' not found for job '{jobId}'.");
        if (status != "Open")
            throw new InvalidOperationException("Only offers on open jobs can be withdrawn.");

        await queries.DeleteAsync(offerId, ct);
    }
}
