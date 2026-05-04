
namespace Job.Marketplace.API.Features.Jobs.Create;

public sealed class CreateJobEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("", async (
                CreateJobRequest request,
                CreateJobHandler handler,
                IValidator<CreateJobRequest> validator,
                CancellationToken ct) =>
        {
            var result = await validator.ValidateAsync(request, ct);
            if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary());
            var response = await handler.HandleAsync(request, ct);
            return Results.Created($"/jobs/{response.Id}", response);
        });
}
