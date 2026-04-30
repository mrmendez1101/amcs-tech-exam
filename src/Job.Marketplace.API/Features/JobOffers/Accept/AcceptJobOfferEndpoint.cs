
namespace Job.Marketplace.API.Features.JobOffers.Accept;

public sealed class AcceptJobOfferEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/jobs/{jobId:guid}/offers/{offerId:guid}/accept", async (
                Guid jobId, Guid offerId, AcceptJobOfferHandler handler, CancellationToken ct) =>
        {
            await handler.HandleAsync(new AcceptJobOfferCommand(jobId, offerId), ct);
            return Results.Ok();
        });
}
