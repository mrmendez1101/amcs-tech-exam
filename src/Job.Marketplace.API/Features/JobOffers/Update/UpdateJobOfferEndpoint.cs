namespace Job.Marketplace.API.Features.JobOffers.Update;

public sealed class UpdateJobOfferEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/jobs/{jobId:guid}/offers/{offerId:guid}", async (
                Guid jobId,
                Guid offerId,
                UpdateJobOfferRequest req,
                UpdateJobOfferHandler handler,
                IValidator<UpdateJobOfferCommand> validator,
                CancellationToken ct) =>
        {
            var cmd = new UpdateJobOfferCommand(jobId, offerId, req.Price);
            var result = await validator.ValidateAsync(cmd, ct);
            if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary());
            var response = await handler.HandleAsync(cmd, ct);
            return Results.Ok(response);
        });
}
