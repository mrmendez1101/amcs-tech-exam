using FluentAssertions;
using Job.Marketplace.API.Features.Jobs.Create;
using NSubstitute;
using DomainJob = Job.Marketplace.Domain.Job;

namespace Job.Marketplace.UnitTests.Features.Jobs;

public class CreateJobHandlerTests
{
    private readonly ICreateJobQueries _queries = Substitute.For<ICreateJobQueries>();

    private static CreateJobRequest ValidRequest(Guid? customerId = null) => new(
        customerId ?? Guid.NewGuid(),
        DateOnly.FromDateTime(DateTime.Today),
        DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
        1000m,
        "Fix the roof");

    [Fact]
    public async Task Throws_KeyNotFoundException_when_customer_not_found()
    {
        _queries.CustomerExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var sut = new CreateJobHandler(_queries);

        Func<Task> act = () => sut.HandleAsync(ValidRequest(), default);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Returns_job_id_on_success()
    {
        var expectedId = Guid.NewGuid();
        _queries.CustomerExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        _queries.InsertJobAsync(Arg.Any<DomainJob>(), Arg.Any<CancellationToken>()).Returns(expectedId);

        var sut = new CreateJobHandler(_queries);
        var response = await sut.HandleAsync(ValidRequest(), default);

        response.Id.Should().Be(expectedId);
    }
}
