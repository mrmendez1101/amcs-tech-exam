using FluentValidation.TestHelper;
using Job.Marketplace.API.Features.Customers.Search;

namespace Job.Marketplace.UnitTests.Features.Customers;

public class SearchCustomersValidatorTests
{
    private readonly SearchCustomersValidator _sut = new();

    [Fact]
    public void Fails_when_term_exceeds_100_chars()
    {
        var result = _sut.TestValidate(new SearchCustomersRequest(new string('x', 101)));
        result.ShouldHaveValidationErrorFor(x => x.Term);
    }

    [Fact]
    public void Fails_when_page_is_zero()
    {
        var result = _sut.TestValidate(new SearchCustomersRequest(null, Page: 0));
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Fails_when_page_size_exceeds_100()
    {
        var result = _sut.TestValidate(new SearchCustomersRequest(null, PageSize: 101));
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Passes_with_valid_query()
    {
        var result = _sut.TestValidate(new SearchCustomersRequest("smi", 1, 20));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
