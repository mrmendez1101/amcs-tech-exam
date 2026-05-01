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

public class JobOffersEndpointTests : IAsyncLifetime
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
    public async Task GetOffers_returns_empty_list_when_job_has_no_offers()
    {
        var customerId = await SeedCustomerAsync();
        var jobId = await CreateJobAsync(customerId);

        var response = await _client.GetAsync($"/jobs/{jobId}/offers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OffersResponse>();
        body!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOffers_returns_offers_for_job()
    {
        var customerId = await SeedCustomerAsync();
        var contractorId = await SeedContractorAsync();
        var jobId = await CreateJobAsync(customerId);
        await CreateOfferAsync(jobId, contractorId);

        var response = await _client.GetAsync($"/jobs/{jobId}/offers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OffersResponse>();
        body!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOffers_returns_404_when_job_not_found()
    {
        var response = await _client.GetAsync($"/jobs/{Guid.NewGuid()}/offers");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteOffer_returns_204_when_offer_exists_and_job_is_open()
    {
        var customerId = await SeedCustomerAsync();
        var contractorId = await SeedContractorAsync();
        var jobId = await CreateJobAsync(customerId);
        var offerId = await CreateOfferAsync(jobId, contractorId);

        var response = await _client.DeleteAsync($"/jobs/{jobId}/offers/{offerId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteOffer_returns_404_when_offer_not_found()
    {
        var customerId = await SeedCustomerAsync();
        var jobId = await CreateJobAsync(customerId);

        var response = await _client.DeleteAsync($"/jobs/{jobId}/offers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteOffer_returns_409_when_job_is_awarded()
    {
        var customerId = await SeedCustomerAsync();
        var contractorId = await SeedContractorAsync();
        var jobId = await CreateJobAsync(customerId);
        var offerId = await CreateOfferAsync(jobId, contractorId);
        await _client.PostAsync($"/jobs/{jobId}/offers/{offerId}/accept", null);

        var response = await _client.DeleteAsync($"/jobs/{jobId}/offers/{offerId}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task<Guid> SeedCustomerAsync()
    {
        var id = Guid.NewGuid();
        using var scope = _factory.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        await using var conn = await factory.CreateAsync();
        await conn.ExecuteAsync(
            "INSERT INTO customers (id, first_name, last_name) VALUES (@id, @f, @l)",
            new { id, f = "Test", l = "User" });
        return id;
    }

    private async Task<Guid> SeedContractorAsync()
    {
        var id = Guid.NewGuid();
        using var scope = _factory.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        await using var conn = await factory.CreateAsync();
        await conn.ExecuteAsync(
            "INSERT INTO contractors (id, name) VALUES (@id, @name)",
            new { id, name = "Test Contractor" });
        return id;
    }

    private async Task<Guid> CreateJobAsync(Guid customerId)
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

    private async Task<Guid> CreateOfferAsync(Guid jobId, Guid contractorId)
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
    private sealed record OfferItem(Guid Id, Guid ContractorId, decimal Price);
    private sealed record OffersResponse(IReadOnlyList<OfferItem> Items);
}
