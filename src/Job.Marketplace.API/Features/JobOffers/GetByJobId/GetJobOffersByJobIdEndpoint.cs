namespace Job.Marketplace.API.Features.JobOffers.GetByJobId;

public sealed class GetJobOffersByJobIdEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("", async (
                Guid jobId,
                GetJobOffersByJobIdHandler handler,
                CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(jobId, ct);
            return Results.Ok(response);
        });
}
