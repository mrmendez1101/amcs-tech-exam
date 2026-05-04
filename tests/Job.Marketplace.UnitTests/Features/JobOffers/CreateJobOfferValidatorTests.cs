using FluentValidation.TestHelper;
using Job.Marketplace.API.Features.JobOffers.Create;

namespace Job.Marketplace.UnitTests.Features.JobOffers;

public class CreateJobOfferValidatorTests
{
    private readonly CreateJobOfferValidator _sut = new();

    [Fact]
    public void Fails_when_job_id_is_empty()
    {
        var result = _sut.TestValidate(new CreateJobOfferCommand(Guid.Empty, Guid.NewGuid(), 100m));
        result.ShouldHaveValidationErrorFor(x => x.JobId);
    }

    [Fact]
    public void Fails_when_contractor_id_is_empty()
    {
        var result = _sut.TestValidate(new CreateJobOfferCommand(Guid.NewGuid(), Guid.Empty, 100m));
        result.ShouldHaveValidationErrorFor(x => x.ContractorId);
    }

    [Fact]
    public void Fails_when_price_is_zero()
    {
        var result = _sut.TestValidate(new CreateJobOfferCommand(Guid.NewGuid(), Guid.NewGuid(), 0m));
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Passes_with_valid_command()
    {
        var result = _sut.TestValidate(new CreateJobOfferCommand(Guid.NewGuid(), Guid.NewGuid(), 500m));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
