using FluentAssertions;
using Job.Marketplace.API.Features.Customers.Search;
using NSubstitute;

namespace Job.Marketplace.UnitTests.Features.Customers;

public class SearchCustomersHandlerTests
{
    [Fact]
    public async Task Returns_paged_results_from_queries()
    {
        var queries = Substitute.For<ISearchCustomersQueries>();
        var dto = new CustomerSummary(Guid.NewGuid(), "John", "Smith", DateTimeOffset.UtcNow);
        queries.SearchAsync("smi", 1, 20, Arg.Any<CancellationToken>())
               .Returns(((IReadOnlyList<CustomerSummary>)[dto], 1));

        var sut = new SearchCustomersHandler(queries);
        var response = await sut.HandleAsync(new SearchCustomersRequest("smi"), default);

        response.Total.Should().Be(1);
        response.Items.Should().ContainSingle().Which.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task Returns_empty_list_when_no_matches()
    {
        var queries = Substitute.For<ISearchCustomersQueries>();
        queries.SearchAsync(Arg.Any<string?>(), 1, 20, Arg.Any<CancellationToken>())
               .Returns(((IReadOnlyList<CustomerSummary>)[], 0));

        var sut = new SearchCustomersHandler(queries);
        var response = await sut.HandleAsync(new SearchCustomersRequest("xyz"), default);

        response.Total.Should().Be(0);
        response.Items.Should().BeEmpty();
    }
}
