namespace Job.Marketplace.Domain;

public sealed class Job
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public decimal Budget { get; private set; }
    public string Description { get; private set; } = default!;
    public Guid? AcceptedOfferId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Job() { }

    public static Job Create(Guid customerId, DateOnly startDate, DateOnly dueDate, decimal budget, string description)
    {
        if (customerId == Guid.Empty) throw new ArgumentException("Customer ID is required.", nameof(customerId));
        if (startDate > dueDate) throw new ArgumentException("Start date must be on or before due date.");
        if (budget <= 0) throw new ArgumentException("Budget must be greater than zero.", nameof(budget));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.", nameof(description));

        return new Job
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            StartDate = startDate,
            DueDate = dueDate,
            Budget = budget,
            Description = description.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
