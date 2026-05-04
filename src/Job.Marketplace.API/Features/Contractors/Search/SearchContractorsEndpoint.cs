
namespace Job.Marketplace.API.Features.Contractors.Search;

public sealed class SearchContractorsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("", async (
                string? term,
                int page,
                int pageSize,
                SearchContractorsHandler handler,
                IValidator<SearchContractorsRequest> validator,
                CancellationToken ct) =>
        {
            var request = new SearchContractorsRequest(term, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize);
            var result = await validator.ValidateAsync(request, ct);
            if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary());
            var response = await handler.HandleAsync(request, ct);
            return Results.Ok(response);
        });
}
