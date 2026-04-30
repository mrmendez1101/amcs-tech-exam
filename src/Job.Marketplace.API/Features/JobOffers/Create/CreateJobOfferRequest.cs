namespace Job.Marketplace.API.Features.JobOffers.Create;

public sealed record CreateJobOfferRequest(Guid ContractorId, decimal Price);

public sealed record CreateJobOfferCommand(Guid JobId, Guid ContractorId, decimal Price);

public sealed class CreateJobOfferValidator : AbstractValidator<CreateJobOfferCommand>
{
    public CreateJobOfferValidator()
    {
        RuleFor(x => x.JobId).NotEmpty();
        RuleFor(x => x.ContractorId).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
