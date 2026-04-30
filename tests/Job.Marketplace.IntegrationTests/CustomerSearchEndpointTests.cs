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

public class CustomerSearchEndpointTests : IAsyncLifetime
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
    public async Task Search_returns_empty_list_when_no_customers()
    {
        var response = await _client.GetAsync("/customers?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SearchResponse>();
        body!.Total.Should().Be(0);
        body.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_returns_customers_matching_prefix()
    {
        await SeedCustomerAsync("Alice", "Smith");
        await SeedCustomerAsync("Bob", "Smithson");
        await SeedCustomerAsync("Carol", "Jones");

        var response = await _client.GetAsync("/customers?term=Smi&page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SearchResponse>();
        body!.Total.Should().Be(2);
        body.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Search_returns_400_when_page_size_exceeds_100()
    {
        var response = await _client.GetAsync("/customers?page=1&pageSize=200");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_returns_customer_when_found()
    {
        var id = await SeedCustomerAsync("Jane", "Doe");

        var response = await _client.GetAsync($"/customers/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_returns_404_when_not_found()
    {
        var response = await _client.GetAsync($"/customers/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

    private sealed record SearchResponse(IReadOnlyList<CustomerItem> Items, int Total);
    private sealed record CustomerItem(Guid Id, string FirstName, string LastName);
}
