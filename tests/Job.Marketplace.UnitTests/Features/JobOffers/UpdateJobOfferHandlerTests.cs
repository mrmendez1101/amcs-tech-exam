using FluentAssertions;
using Job.Marketplace.API.Features.JobOffers.Update;
using NSubstitute;

namespace Job.Marketplace.UnitTests.Features.JobOffers;

public class UpdateJobOfferHandlerTests
{
    [Fact]
    public async Task Returns_offer_id_when_offer_exists()
    {
        var jobId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var queries = Substitute.For<IUpdateJobOfferQueries>();
        queries.OfferExistsAsync(jobId, offerId, Arg.Any<CancellationToken>()).Returns(true);

        var sut = new UpdateJobOfferHandler(queries);
        var response = await sut.HandleAsync(new UpdateJobOfferCommand(jobId, offerId, 900m), default);

        response.Id.Should().Be(offerId);
        await queries.Received(1).UpdateAsync(Arg.Any<UpdateJobOfferCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_KeyNotFoundException_when_offer_not_found()
    {
        var jobId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var queries = Substitute.For<IUpdateJobOfferQueries>();
        queries.OfferExistsAsync(jobId, offerId, Arg.Any<CancellationToken>()).Returns(false);

        var sut = new UpdateJobOfferHandler(queries);
        Func<Task> act = () => sut.HandleAsync(new UpdateJobOfferCommand(jobId, offerId, 900m), default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await queries.DidNotReceive().UpdateAsync(Arg.Any<UpdateJobOfferCommand>(), Arg.Any<CancellationToken>());
    }
}
