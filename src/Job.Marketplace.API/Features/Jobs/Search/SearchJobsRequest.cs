namespace Job.Marketplace.API.Features.Jobs.Search;

public sealed record SearchJobsRequest(string? Term, int Page = 1, int PageSize = 20);

public sealed class SearchJobsValidator : AbstractValidator<SearchJobsRequest>
{
    public SearchJobsValidator()
    {
        RuleFor(x => x.Term).MaximumLength(100);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
