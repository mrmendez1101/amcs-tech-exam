using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Job.Marketplace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Job.Marketplace.IntegrationTests;

public class JobsEndpointTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder("postgres:15-alpine")
        .Build();

    private WebApplicationFactory<Program> _factory = default!;
    private HttpClient _client = default!;

    public async Task InitializeAsync()
    {
        await _pg.StartAsync();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
            b.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Marketplace"] = _pg.GetConnectionString()
            })));
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _pg.DisposeAsync();
    }

    [Fact]
    public async Task CreateJob_returns_201_with_id()
    {
        var customerId = await SeedCustomerAsync("John", "Doe");

        var response = await _client.PostAsJsonAsync("/jobs", new
        {
            customerId,
            startDate = "2026-05-01",
            dueDate = "2026-05-31",
            budget = 5000,
            description = "Fix the roof"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateJob_returns_404_when_customer_not_found()
    {
        var response = await _client.PostAsJsonAsync("/jobs", new
        {
            customerId = Guid.NewGuid(),
            startDate = "2026-05-01",
            dueDate = "2026-05-31",
            budget = 5000,
            description = "Fix the roof"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetJob_returns_200_when_found()
    {
        var customerId = await SeedCustomerAsync("Jane", "Doe");
        var jobId = await CreateJobViaApiAsync(customerId);

        var response = await _client.GetAsync($"/jobs/{jobId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetJob_returns_404_when_not_found()
    {
        var response = await _client.GetAsync($"/jobs/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteJob_returns_409_when_job_is_awarded()
    {
        var customerId = await SeedCustomerAsync("Bob", "Smith");
        var contractorId = await SeedContractorAsync("Acme Ltd");
        var jobId = await CreateJobViaApiAsync(customerId);
        var offerId = await CreateOfferViaApiAsync(jobId, contractorId);
        await _client.PostAsync($"/jobs/{jobId}/offers/{offerId}/accept", null);

        var response = await _client.DeleteAsync($"/jobs/{jobId}");
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task<Guid> SeedCustomerAsync(string firstName, string lastName)
    {
        var id = Guid.NewGuid();
        using var scope = _factory.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        await using var conn = await factory.CreateAsync();
        await conn.ExecuteAsync(
            "INSERT INTO customers (id, first_name, last_name) VALUES (@id, @firstName, @lastName)",
            new { id, firstName, lastName });
        return id;
    }

    private async Task<Guid> SeedContractorAsync(string name)
    {
        var id = Guid.NewGuid();
        using var scope = _factory.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        await using var conn = await factory.CreateAsync();
        await conn.ExecuteAsync(
            "INSERT INTO contractors (id, name) VALUES (@id, @name)",
            new { id, name });
        return id;
    }

    private async Task<Guid> CreateJobViaApiAsync(Guid customerId)
    {
        var response = await _client.PostAsJsonAsync("/jobs", new
        {
            customerId,
            startDate = "2026-05-01",
            dueDate = "2026-05-31",
            budget = 1000,
            description = "Test job"
        });
        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        return body!.Id;
    }

    private async Task<Guid> CreateOfferViaApiAsync(Guid jobId, Guid contractorId)
    {
        var response = await _client.PostAsJsonAsync($"/jobs/{jobId}/offers", new
        {
            contractorId,
            price = 800
        });
        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        return body!.Id;
    }

    private sealed record IdResponse(Guid Id);
}
