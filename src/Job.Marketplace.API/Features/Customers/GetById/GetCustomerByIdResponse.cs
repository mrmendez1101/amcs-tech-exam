namespace Job.Marketplace.API.Features.Customers.GetById;

public sealed record CustomerDetail(Guid Id, string FirstName, string LastName, DateTimeOffset CreatedAt);
