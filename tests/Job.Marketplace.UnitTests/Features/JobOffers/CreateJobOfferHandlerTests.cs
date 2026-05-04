using FluentAssertions;
using Job.Marketplace.API.Features.JobOffers.Create;
using Job.Marketplace.Domain;
using NSubstitute;

namespace Job.Marketplace.UnitTests.Features.JobOffers;

public class CreateJobOfferHandlerTests
{
    private readonly ICreateJobOfferQueries _queries = Substitute.For<ICreateJobOfferQueries>();

    [Fact]
    public async Task Throws_KeyNotFoundException_when_job_not_found()
    {
        _queries.JobExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var sut = new CreateJobOfferHandler(_queries);
        Func<Task> act = () => sut.HandleAsync(
            new CreateJobOfferCommand(Guid.NewGuid(), Guid.NewGuid(), 500m), default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await _queries.DidNotReceive().InsertOfferAsync(Arg.Any<JobOffer>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_KeyNotFoundException_when_contractor_not_found()
    {
        _queries.JobExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        _queries.ContractorExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var sut = new CreateJobOfferHandler(_queries);
        Func<Task> act = () => sut.HandleAsync(
            new CreateJobOfferCommand(Guid.NewGuid(), Guid.NewGuid(), 500m), default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await _queries.DidNotReceive().InsertOfferAsync(Arg.Any<JobOffer>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_offer_id_on_success()
    {
        var expectedId = Guid.NewGuid();
        _queries.JobExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        _queries.ContractorExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        _queries.InsertOfferAsync(Arg.Any<JobOffer>(), Arg.Any<CancellationToken>()).Returns(expectedId);

        var sut = new CreateJobOfferHandler(_queries);
        var response = await sut.HandleAsync(
            new CreateJobOfferCommand(Guid.NewGuid(), Guid.NewGuid(), 500m), default);

        response.Id.Should().Be(expectedId);
    }
}
