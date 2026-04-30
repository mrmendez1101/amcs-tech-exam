
namespace Job.Marketplace.API.Features.Customers.GetById;

public sealed class GetCustomerByIdEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/customers/{id:guid}", async (
                Guid id, GetCustomerByIdHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(id, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });
}
