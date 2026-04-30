using DomainJob = Job.Marketplace.Domain.Job;

namespace Job.Marketplace.API.Features.Jobs.Create;

public sealed record CreateJobRequest(Guid CustomerId, DateOnly StartDate, DateOnly DueDate, decimal Budget, string Description);

public sealed class CreateJobValidator : AbstractValidator<CreateJobRequest>
{
    public CreateJobValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("Due date must be on or after start date.");
        RuleFor(x => x.Budget).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}
