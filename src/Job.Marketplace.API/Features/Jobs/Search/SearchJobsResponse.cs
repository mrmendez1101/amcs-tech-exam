namespace Job.Marketplace.API.Features.Jobs.Search;

public sealed record JobSummary(
    Guid Id,
    Guid CustomerId,
    DateOnly StartDate,
    DateOnly DueDate,
    decimal Budget,
    string Description,
    Guid? AcceptedOfferId,
    DateTimeOffset CreatedAt);

public sealed record SearchJobsResponse(IReadOnlyList<JobSummary> Items, int Total);
