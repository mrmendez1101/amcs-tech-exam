
namespace Job.Marketplace.API.Features.Customers.Search;

public sealed class SearchCustomersEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("", async (
                string? term,
                int page,
                int pageSize,
                SearchCustomersHandler handler,
                IValidator<SearchCustomersRequest> validator,
                CancellationToken ct) =>
        {
            var request = new SearchCustomersRequest(term, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize);
            var result = await validator.ValidateAsync(request, ct);
            if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary());
            var response = await handler.HandleAsync(request, ct);
            return Results.Ok(response);
        });
}
