namespace Job.Marketplace.API.Features.Customers.Search;

public sealed record SearchCustomersRequest(string? Term, int Page = 1, int PageSize = 20);

public sealed class SearchCustomersValidator : AbstractValidator<SearchCustomersRequest>
{
    public SearchCustomersValidator()
    {
        RuleFor(x => x.Term).MaximumLength(100);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
