namespace Job.Marketplace.API.Features.Customers.Search;

public sealed class SearchCustomersHandler(ISearchCustomersQueries queries)
{
    public async Task<SearchCustomersResponse> HandleAsync(SearchCustomersRequest request, CancellationToken ct)
    {
        var (items, total) = await queries.SearchAsync(request.Term, request.Page, request.PageSize, ct);
        return new SearchCustomersResponse(items, total);
    }
}
