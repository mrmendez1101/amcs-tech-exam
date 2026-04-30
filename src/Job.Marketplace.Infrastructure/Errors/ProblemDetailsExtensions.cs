using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Job.Marketplace.Infrastructure.Errors;

public static class ProblemDetailsExtensions
{
    public static IServiceCollection AddMarketplaceProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(opts =>
        {
            opts.CustomizeProblemDetails = ctx =>
            {
                if (ctx.Exception is InvalidOperationException)
                {
                    ctx.ProblemDetails.Status = StatusCodes.Status409Conflict;
                    ctx.ProblemDetails.Title = "Business rule violation.";
                    ctx.ProblemDetails.Detail = ctx.Exception.Message;
                    ctx.HttpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                }
                else if (ctx.Exception is KeyNotFoundException)
                {
                    ctx.ProblemDetails.Status = StatusCodes.Status404NotFound;
                    ctx.ProblemDetails.Title = "Resource not found.";
                    ctx.ProblemDetails.Detail = ctx.Exception.Message;
                    ctx.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                }
            };
        });
        return services;
    }
}
