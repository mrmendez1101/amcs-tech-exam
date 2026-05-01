using FluentAssertions;
using Job.Marketplace.API.Features.JobOffers.Delete;
using NSubstitute;

namespace Job.Marketplace.UnitTests.Features.JobOffers;

public class DeleteJobOfferHandlerTests
{
    [Fact]
    public async Task Deletes_offer_when_it_exists()
    {
        var jobId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var queries = Substitute.For<IDeleteJobOfferQueries>();
        queries.OfferExistsAsync(jobId, offerId, Arg.Any<CancellationToken>()).Returns(true);

        var sut = new DeleteJobOfferHandler(queries);
        await sut.HandleAsync(jobId, offerId, default);

        await queries.Received(1).DeleteAsync(offerId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_KeyNotFoundException_when_offer_not_found()
    {
        var jobId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var queries = Substitute.For<IDeleteJobOfferQueries>();
        queries.OfferExistsAsync(jobId, offerId, Arg.Any<CancellationToken>()).Returns(false);

        var sut = new DeleteJobOfferHandler(queries);
        Func<Task> act = () => sut.HandleAsync(jobId, offerId, default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await queries.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
