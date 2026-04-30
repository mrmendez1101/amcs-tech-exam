namespace Job.Marketplace.API.Features.Contractors.Search;

public sealed record SearchContractorsRequest(string? Term, int Page = 1, int PageSize = 20);

public sealed class SearchContractorsValidator : AbstractValidator<SearchContractorsRequest>
{
    public SearchContractorsValidator()
    {
        RuleFor(x => x.Term).MaximumLength(100);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
