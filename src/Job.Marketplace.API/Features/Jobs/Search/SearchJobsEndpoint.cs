namespace Job.Marketplace.API.Features.Jobs.Search;

public sealed class SearchJobsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/jobs", async (
                string? term, int page, int pageSize,
                SearchJobsHandler handler,
                IValidator<SearchJobsRequest> validator,
                CancellationToken ct) =>
        {
            var request = new SearchJobsRequest(term, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize);
            var result = await validator.ValidateAsync(request, ct);
            if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary());
            var response = await handler.HandleAsync(request, ct);
            return Results.Ok(response);
        });
}
