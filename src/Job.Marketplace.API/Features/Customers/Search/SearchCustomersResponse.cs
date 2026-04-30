namespace Job.Marketplace.API.Features.Customers.Search;

public sealed record CustomerSummary(Guid Id, string FirstName, string LastName, DateTimeOffset CreatedAt);

public sealed record SearchCustomersResponse(IReadOnlyList<CustomerSummary> Items, int Total);
