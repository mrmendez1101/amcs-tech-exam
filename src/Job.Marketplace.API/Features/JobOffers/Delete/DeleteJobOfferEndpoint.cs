namespace Job.Marketplace.API.Features.JobOffers.Delete;

public sealed class DeleteJobOfferEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/jobs/{jobId:guid}/offers/{offerId:guid}", async (
                Guid jobId,
                Guid offerId,
                DeleteJobOfferHandler handler,
                CancellationToken ct) =>
        {
            await handler.HandleAsync(jobId, offerId, ct);
            return Results.NoContent();
        });
}
