namespace Job.Marketplace.API.Features.JobOffers.GetByJobId;

public sealed class GetJobOffersByJobIdHandler(IGetJobOffersByJobIdQueries queries)
{
    public async Task<GetJobOffersByJobIdResponse> HandleAsync(Guid jobId, CancellationToken ct)
    {
        if (!await queries.JobExistsAsync(jobId, ct))
            throw new KeyNotFoundException($"Job '{jobId}' not found.");

        var items = await queries.GetByJobIdAsync(jobId, ct);
        return new GetJobOffersByJobIdResponse(items);
    }
}
