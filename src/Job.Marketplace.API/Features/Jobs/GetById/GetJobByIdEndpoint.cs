
namespace Job.Marketplace.API.Features.Jobs.GetById;

public sealed class GetJobByIdEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/jobs/{id:guid}", async (
                Guid id, GetJobByIdHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(id, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });
}
