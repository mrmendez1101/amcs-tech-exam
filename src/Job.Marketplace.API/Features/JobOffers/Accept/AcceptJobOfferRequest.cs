namespace Job.Marketplace.API.Features.JobOffers.Accept;

public sealed record AcceptJobOfferRequest(Guid CustomerId);

public sealed class AcceptJobOfferValidator : AbstractValidator<AcceptJobOfferRequest>
{
    public AcceptJobOfferValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
