namespace Job.Marketplace.API.Features.Jobs.Search;

public sealed class SearchJobsHandler(ISearchJobsQueries queries)
{
    public async Task<SearchJobsResponse> HandleAsync(SearchJobsRequest request, CancellationToken ct)
    {
        var (items, total) = await queries.SearchAsync(request.Term, request.Page, request.PageSize, ct);
        return new SearchJobsResponse(items, total);
    }
}
