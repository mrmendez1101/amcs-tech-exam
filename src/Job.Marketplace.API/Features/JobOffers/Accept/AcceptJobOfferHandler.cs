using DomainJob = Job.Marketplace.Domain.Job;

namespace Job.Marketplace.API.Features.JobOffers.Accept;

public sealed class AcceptJobOfferHandler(IAcceptJobOfferQueries queries)
{
    public async Task HandleAsync(AcceptJobOfferCommand cmd, CancellationToken ct)
    {
        var snapshot = await queries.GetJobSnapshotAsync(cmd.JobId, ct);
        if (snapshot is null)
            throw new KeyNotFoundException($"Job '{cmd.JobId}' not found.");

        if (!await queries.OfferBelongsToJobAsync(cmd.JobId, cmd.OfferId, ct))
            throw new KeyNotFoundException($"Offer '{cmd.OfferId}' not found for job '{cmd.JobId}'.");

        var job = DomainJob.Reconstitute(snapshot.Id, snapshot.Status);
        job.AcceptOffer(cmd.OfferId);

        await queries.AcceptAsync(cmd.JobId, cmd.OfferId, ct);
    }
}
