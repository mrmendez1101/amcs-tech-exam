namespace Job.Marketplace.API.Features.Contractors.Search;

public sealed class SearchContractorsHandler(ISearchContractorsQueries queries)
{
    public async Task<SearchContractorsResponse> HandleAsync(SearchContractorsRequest request, CancellationToken ct)
    {
        var (items, total) = await queries.SearchAsync(request.Term, request.Page, request.PageSize, ct);
        return new SearchContractorsResponse(items, total);
    }
}
