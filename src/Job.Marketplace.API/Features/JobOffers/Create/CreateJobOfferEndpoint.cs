
namespace Job.Marketplace.API.Features.JobOffers.Create;

public sealed class CreateJobOfferEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/jobs/{jobId:guid}/offers", async (
                Guid jobId,
                CreateJobOfferRequest req,
                CreateJobOfferHandler handler,
                IValidator<CreateJobOfferCommand> validator,
                CancellationToken ct) =>
        {
            var cmd = new CreateJobOfferCommand(jobId, req.ContractorId, req.Price);
            var result = await validator.ValidateAsync(cmd, ct);
            if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary());
            var response = await handler.HandleAsync(cmd, ct);
            return Results.Created($"/jobs/{jobId}/offers/{response.Id}", response);
        });
}
