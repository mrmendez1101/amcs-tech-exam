namespace Job.Marketplace.API.Features.JobOffers.Accept;

public sealed class AcceptJobOfferEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/{offerId:guid}/accept", async (
                Guid jobId,
                Guid offerId,
                AcceptJobOfferRequest req,
                AcceptJobOfferHandler handler,
                IValidator<AcceptJobOfferRequest> validator,
                CancellationToken ct) =>
        {
            var result = await validator.ValidateAsync(req, ct);
            if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary());
            await handler.HandleAsync(new AcceptJobOfferCommand(jobId, offerId, req.CustomerId), ct);
            return Results.Ok();
        });
}
