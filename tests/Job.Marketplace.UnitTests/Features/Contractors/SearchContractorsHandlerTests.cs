using FluentAssertions;
using Job.Marketplace.API.Features.Contractors.Search;
using NSubstitute;

namespace Job.Marketplace.UnitTests.Features.Contractors;

public class SearchContractorsHandlerTests
{
    [Fact]
    public async Task Returns_paged_results_from_queries()
    {
        var queries = Substitute.For<ISearchContractorsQueries>();
        var dto = new ContractorSummary(Guid.NewGuid(), "Acme Corp", 4.5m, DateTimeOffset.UtcNow);
        queries.SearchAsync("acme", 1, 20, Arg.Any<CancellationToken>())
               .Returns(((IReadOnlyList<ContractorSummary>)[dto], 1));

        var sut = new SearchContractorsHandler(queries);
        var response = await sut.HandleAsync(new SearchContractorsRequest("acme"), default);

        response.Total.Should().Be(1);
        response.Items.Should().ContainSingle().Which.Name.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task Returns_empty_list_when_no_matches()
    {
        var queries = Substitute.For<ISearchContractorsQueries>();
        queries.SearchAsync(Arg.Any<string?>(), 1, 20, Arg.Any<CancellationToken>())
               .Returns(((IReadOnlyList<ContractorSummary>)[], 0));

        var sut = new SearchContractorsHandler(queries);
        var response = await sut.HandleAsync(new SearchContractorsRequest("xyz"), default);

        response.Total.Should().Be(0);
        response.Items.Should().BeEmpty();
    }
}
