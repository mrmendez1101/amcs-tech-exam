namespace Job.Marketplace.API.Features.Contractors.Search;

public sealed record ContractorSummary(Guid Id, string Name, decimal Rating, DateTimeOffset CreatedAt);

public sealed record SearchContractorsResponse(IReadOnlyList<ContractorSummary> Items, int Total);
