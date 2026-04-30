using Job.Marketplace.Infrastructure.Errors;
using Job.Marketplace.Infrastructure.Persistence;
using Job.Marketplace.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Job.Marketplace.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        DapperConfig.Apply();
        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddMarketplaceProblemDetails();
        return services;
    }
}
