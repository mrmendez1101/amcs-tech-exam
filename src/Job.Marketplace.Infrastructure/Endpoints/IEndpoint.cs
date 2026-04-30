using Microsoft.AspNetCore.Routing;

namespace Job.Marketplace.Infrastructure.Endpoints;

public interface IEndpoint
{
    static abstract void Map(IEndpointRouteBuilder app);
}
