
namespace Job.Marketplace.API.Features.Jobs.Update;

public sealed class UpdateJobEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/jobs/{id:guid}", async (
                Guid id,
                UpdateJobRequest req,
                UpdateJobHandler handler,
                IValidator<UpdateJobCommand> validator,
                CancellationToken ct) =>
        {
            var cmd = new UpdateJobCommand(id, req.StartDate, req.DueDate, req.Budget, req.Description);
            var result = await validator.ValidateAsync(cmd, ct);
            if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary());
            var response = await handler.HandleAsync(cmd, ct);
            return Results.Ok(response);
        });
}
