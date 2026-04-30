namespace Job.Marketplace.API.Features.Jobs.Update;

public sealed record UpdateJobRequest(DateOnly StartDate, DateOnly DueDate, decimal Budget, string Description);

public sealed record UpdateJobCommand(Guid Id, DateOnly StartDate, DateOnly DueDate, decimal Budget, string Description);

public sealed class UpdateJobValidator : AbstractValidator<UpdateJobCommand>
{
    public UpdateJobValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("Due date must be on or after start date.");
        RuleFor(x => x.Budget).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}
