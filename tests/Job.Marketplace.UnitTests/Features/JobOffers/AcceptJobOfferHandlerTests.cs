using FluentAssertions;
using Job.Marketplace.API.Features.JobOffers.Accept;
using NSubstitute;

namespace Job.Marketplace.UnitTests.Features.JobOffers;

public class AcceptJobOfferHandlerTests
{
    private static AcceptJobOfferCommand MakeCmd(Guid jobId, Guid offerId, Guid customerId)
        => new(jobId, offerId, customerId);

    [Fact]
    public async Task Throws_404_when_job_not_found()
    {
        var queries = Substitute.For<IAcceptJobOfferQueries>();
        queries.GetJobSnapshotAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
               .Returns((JobSnapshot?)null);

        var sut = new AcceptJobOfferHandler(queries);
        Func<Task> act = () => sut.HandleAsync(
            MakeCmd(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Throws_404_when_customer_does_not_own_job()
    {
        var jobId = Guid.NewGuid();
        var ownerCustomerId = Guid.NewGuid();
        var queries = Substitute.For<IAcceptJobOfferQueries>();
        queries.GetJobSnapshotAsync(jobId, Arg.Any<CancellationToken>())
               .Returns(new JobSnapshot(jobId, ownerCustomerId, null));

        var sut = new AcceptJobOfferHandler(queries);
        Func<Task> act = () => sut.HandleAsync(
            MakeCmd(jobId, Guid.NewGuid(), Guid.NewGuid()), default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Throws_409_when_job_already_accepted()
    {
        var jobId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var queries = Substitute.For<IAcceptJobOfferQueries>();
        queries.GetJobSnapshotAsync(jobId, Arg.Any<CancellationToken>())
               .Returns(new JobSnapshot(jobId, customerId, Guid.NewGuid()));

        var sut = new AcceptJobOfferHandler(queries);
        Func<Task> act = () => sut.HandleAsync(
            MakeCmd(jobId, Guid.NewGuid(), customerId), default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Throws_404_when_offer_not_for_job()
    {
        var jobId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var queries = Substitute.For<IAcceptJobOfferQueries>();
        queries.GetJobSnapshotAsync(jobId, Arg.Any<CancellationToken>())
               .Returns(new JobSnapshot(jobId, customerId, null));
        queries.OfferBelongsToJobAsync(jobId, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
               .Returns(false);

        var sut = new AcceptJobOfferHandler(queries);
        Func<Task> act = () => sut.HandleAsync(
            MakeCmd(jobId, Guid.NewGuid(), customerId), default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Calls_AcceptAsync_when_customer_owns_open_job_and_offer_matches()
    {
        var jobId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var queries = Substitute.For<IAcceptJobOfferQueries>();
        queries.GetJobSnapshotAsync(jobId, Arg.Any<CancellationToken>())
               .Returns(new JobSnapshot(jobId, customerId, null));
        queries.OfferBelongsToJobAsync(jobId, offerId, Arg.Any<CancellationToken>())
               .Returns(true);

        var sut = new AcceptJobOfferHandler(queries);
        await sut.HandleAsync(MakeCmd(jobId, offerId, customerId), default);

        await queries.Received(1).AcceptAsync(jobId, offerId, Arg.Any<CancellationToken>());
    }
}
