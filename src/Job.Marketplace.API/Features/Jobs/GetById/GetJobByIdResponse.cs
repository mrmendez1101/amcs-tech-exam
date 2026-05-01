namespace Job.Marketplace.API.Features.Jobs.GetById;

public sealed record JobDetail(
    Guid Id,
    Guid CustomerId,
    DateOnly StartDate,
    DateOnly DueDate,
    decimal Budget,
    string Description,
    Guid? AcceptedOfferId,
    DateTimeOffset CreatedAt);
