using FluentValidation.TestHelper;
using Job.Marketplace.API.Features.Jobs.Create;

namespace Job.Marketplace.UnitTests.Features.Jobs;

public class CreateJobValidatorTests
{
    private readonly CreateJobValidator _sut = new();

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    [Fact]
    public void Fails_when_customer_id_is_empty()
    {
        var result = _sut.TestValidate(new CreateJobRequest(Guid.Empty, Today, Today.AddDays(1), 500m, "Desc"));
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Fails_when_due_date_before_start_date()
    {
        var result = _sut.TestValidate(new CreateJobRequest(Guid.NewGuid(), Today.AddDays(5), Today, 500m, "Desc"));
        result.ShouldHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public void Fails_when_budget_is_zero()
    {
        var result = _sut.TestValidate(new CreateJobRequest(Guid.NewGuid(), Today, Today.AddDays(1), 0m, "Desc"));
        result.ShouldHaveValidationErrorFor(x => x.Budget);
    }

    [Fact]
    public void Fails_when_description_is_empty()
    {
        var result = _sut.TestValidate(new CreateJobRequest(Guid.NewGuid(), Today, Today.AddDays(1), 500m, ""));
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Fails_when_description_exceeds_500_chars()
    {
        var result = _sut.TestValidate(new CreateJobRequest(Guid.NewGuid(), Today, Today.AddDays(1), 500m, new string('x', 501)));
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Passes_with_valid_command()
    {
        var result = _sut.TestValidate(new CreateJobRequest(Guid.NewGuid(), Today, Today.AddDays(7), 1000m, "Fix the roof"));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
