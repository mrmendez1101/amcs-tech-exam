namespace Job.Marketplace.API.Features.JobOffers.Create;

public sealed class CreateJobOfferHandler(ICreateJobOfferQueries queries)
{
    public async Task<CreateJobOfferResponse> HandleAsync(CreateJobOfferCommand cmd, CancellationToken ct)
    {
        if (!await queries.JobExistsAsync(cmd.JobId, ct))
            throw new KeyNotFoundException($"Job '{cmd.JobId}' not found.");

        if (!await queries.ContractorExistsAsync(cmd.ContractorId, ct))
            throw new KeyNotFoundException($"Contractor '{cmd.ContractorId}' not found.");

        var offer = JobOffer.Create(cmd.JobId, cmd.ContractorId, cmd.Price);
        var id = await queries.InsertOfferAsync(offer, ct);
        return new CreateJobOfferResponse(id);
    }
}
