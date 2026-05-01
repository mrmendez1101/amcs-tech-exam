using FluentAssertions;
using Job.Marketplace.API.Features.JobOffers.GetByJobId;
using NSubstitute;

namespace Job.Marketplace.UnitTests.Features.JobOffers;

public class GetJobOffersByJobIdHandlerTests
{
    [Fact]
    public async Task Returns_offers_when_job_exists()
    {
        var jobId = Guid.NewGuid();
        var queries = Substitute.For<IGetJobOffersByJobIdQueries>();
        var offer = new JobOfferSummary(Guid.NewGuid(), Guid.NewGuid(), 500m, DateTimeOffset.UtcNow);
        queries.JobExistsAsync(jobId, Arg.Any<CancellationToken>()).Returns(true);
        queries.GetByJobIdAsync(jobId, Arg.Any<CancellationToken>())
               .Returns((IReadOnlyList<JobOfferSummary>)[offer]);

        var sut = new GetJobOffersByJobIdHandler(queries);
        var response = await sut.HandleAsync(jobId, default);

        response.Items.Should().ContainSingle().Which.Id.Should().Be(offer.Id);
    }

    [Fact]
    public async Task Throws_KeyNotFoundException_when_job_not_found()
    {
        var jobId = Guid.NewGuid();
        var queries = Substitute.For<IGetJobOffersByJobIdQueries>();
        queries.JobExistsAsync(jobId, Arg.Any<CancellationToken>()).Returns(false);

        var sut = new GetJobOffersByJobIdHandler(queries);
        Func<Task> act = () => sut.HandleAsync(jobId, default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
