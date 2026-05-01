namespace Job.Marketplace.API.Features.JobOffers.Update;

public sealed class UpdateJobOfferHandler(IUpdateJobOfferQueries queries)
{
    public async Task<UpdateJobOfferResponse> HandleAsync(UpdateJobOfferCommand cmd, CancellationToken ct)
    {
        if (!await queries.OfferExistsAsync(cmd.JobId, cmd.OfferId, ct))
            throw new KeyNotFoundException($"Offer '{cmd.OfferId}' not found for job '{cmd.JobId}'.");

        await queries.UpdateAsync(cmd, ct);
        return new UpdateJobOfferResponse(cmd.OfferId);
    }
}
