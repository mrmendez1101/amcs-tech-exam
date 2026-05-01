namespace Job.Marketplace.API.Features.JobOffers.Update;

public sealed record UpdateJobOfferRequest(decimal Price);

public sealed record UpdateJobOfferCommand(Guid JobId, Guid OfferId, decimal Price);

public sealed class UpdateJobOfferValidator : AbstractValidator<UpdateJobOfferCommand>
{
    public UpdateJobOfferValidator()
    {
        RuleFor(x => x.JobId).NotEmpty();
        RuleFor(x => x.OfferId).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
