using FluentAssertions;
using FluentValidation.TestHelper;
using Job.Marketplace.API.Features.Jobs.Search;

namespace Job.Marketplace.UnitTests.Features.Jobs;

public class SearchJobsValidatorTests
{
    private readonly SearchJobsValidator _validator = new();

    [Fact]
    public void Passes_with_valid_request()
    {
        var result = _validator.TestValidate(new SearchJobsRequest("roof", 1, 20));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Fails_when_page_size_exceeds_100()
    {
        var result = _validator.TestValidate(new SearchJobsRequest(null, 1, 200));
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Fails_when_page_is_zero()
    {
        var result = _validator.TestValidate(new SearchJobsRequest(null, 0, 20));
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Fails_when_term_exceeds_100_chars()
    {
        var result = _validator.TestValidate(new SearchJobsRequest(new string('x', 101), 1, 20));
        result.ShouldHaveValidationErrorFor(x => x.Term);
    }
}
