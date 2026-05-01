using FluentAssertions;
using Job.Marketplace.API.Features.Jobs.Search;
using NSubstitute;

namespace Job.Marketplace.UnitTests.Features.Jobs;

public class SearchJobsHandlerTests
{
    [Fact]
    public async Task Returns_paged_results_from_queries()
    {
        var queries = Substitute.For<ISearchJobsQueries>();
        var job = new JobSummary(Guid.NewGuid(), Guid.NewGuid(),
            new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31),
            1000m, "Fix the roof", null, DateTimeOffset.UtcNow);
        queries.SearchAsync("roof", 1, 20, Arg.Any<CancellationToken>())
               .Returns(((IReadOnlyList<JobSummary>)[job], 1));

        var sut = new SearchJobsHandler(queries);
        var response = await sut.HandleAsync(new SearchJobsRequest("roof"), default);

        response.Total.Should().Be(1);
        response.Items.Should().ContainSingle().Which.Description.Should().Be("Fix the roof");
    }

    [Fact]
    public async Task Returns_empty_list_when_no_matches()
    {
        var queries = Substitute.For<ISearchJobsQueries>();
        queries.SearchAsync(Arg.Any<string?>(), 1, 20, Arg.Any<CancellationToken>())
               .Returns(((IReadOnlyList<JobSummary>)[], 0));

        var sut = new SearchJobsHandler(queries);
        var response = await sut.HandleAsync(new SearchJobsRequest("xyz"), default);

        response.Total.Should().Be(0);
        response.Items.Should().BeEmpty();
    }
}
