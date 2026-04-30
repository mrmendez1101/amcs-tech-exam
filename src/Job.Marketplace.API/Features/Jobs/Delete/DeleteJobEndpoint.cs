
namespace Job.Marketplace.API.Features.Jobs.Delete;

public sealed class DeleteJobEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/jobs/{id:guid}", async (
                Guid id, DeleteJobHandler handler, CancellationToken ct) =>
        {
            await handler.HandleAsync(id, ct);
            return Results.NoContent();
        });
}
