namespace Job.Marketplace.API.Features.JobOffers.GetByJobId;

public sealed record JobOfferSummary(Guid Id, Guid ContractorId, decimal Price, DateTimeOffset CreatedAt);

public sealed record GetJobOffersByJobIdResponse(IReadOnlyList<JobOfferSummary> Items);
