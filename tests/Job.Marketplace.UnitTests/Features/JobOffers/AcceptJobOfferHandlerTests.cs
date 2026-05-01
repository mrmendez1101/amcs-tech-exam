using FluentAssertions;
using Job.Marketplace.API.Features.JobOffers.Accept;
using NSubstitute;

namespace Job.Marketplace.UnitTests.Features.JobOffers;

public class AcceptJobOfferHandlerTests
{
    private readonly IAcceptJobOfferQueries _queries = Substitute.For<IAcceptJobOfferQueries>();

    [Fact]
    public async Task Throws_when_job_not_found()
    {
        _queries.JobExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(false);

        var sut = new AcceptJobOfferHandler(_queries);

        Func<Task> act = () => sut.HandleAsync(new AcceptJobOfferCommand(Guid.NewGuid(), Guid.NewGuid()), default);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Throws_when_offer_not_found_for_job()
    {
        var jobId = Guid.NewGuid();
        _queries.JobExistsAsync(jobId, Arg.Any<CancellationToken>())
                .Returns(true);
        _queries.OfferBelongsToJobAsync(jobId, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(false);

        var sut = new AcceptJobOfferHandler(_queries);

        Func<Task> act = () => sut.HandleAsync(new AcceptJobOfferCommand(jobId, Guid.NewGuid()), default);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Calls_AcceptAsync_for_valid_job_and_offer()
    {
        var jobId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        _queries.JobExistsAsync(jobId, Arg.Any<CancellationToken>())
                .Returns(true);
        _queries.OfferBelongsToJobAsync(jobId, offerId, Arg.Any<CancellationToken>())
                .Returns(true);

        var sut = new AcceptJobOfferHandler(_queries);
        await sut.HandleAsync(new AcceptJobOfferCommand(jobId, offerId), default);

        await _queries.Received(1).AcceptAsync(jobId, offerId, Arg.Any<CancellationToken>());
    }
}
