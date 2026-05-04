using FluentValidation.TestHelper;
using Job.Marketplace.API.Features.Contractors.Search;

namespace Job.Marketplace.UnitTests.Features.Contractors;

public class SearchContractorsValidatorTests
{
    private readonly SearchContractorsValidator _sut = new();

    [Fact]
    public void Fails_when_term_exceeds_100_chars()
    {
        var result = _sut.TestValidate(new SearchContractorsRequest(new string('x', 101)));
        result.ShouldHaveValidationErrorFor(x => x.Term);
    }

    [Fact]
    public void Fails_when_page_is_zero()
    {
        var result = _sut.TestValidate(new SearchContractorsRequest(null, Page: 0));
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Fails_when_page_size_exceeds_100()
    {
        var result = _sut.TestValidate(new SearchContractorsRequest(null, PageSize: 101));
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Passes_with_valid_query()
    {
        var result = _sut.TestValidate(new SearchContractorsRequest("acme", 1, 20));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
