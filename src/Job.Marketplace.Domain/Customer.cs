namespace Job.Marketplace.Domain;

public sealed class Customer
{
    public Guid Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public DateTimeOffset CreatedAt { get; }

    private Customer(Guid id, string firstName, string lastName, DateTimeOffset createdAt)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        CreatedAt = createdAt;
    }

    public static Customer Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));
        return new Customer(Guid.NewGuid(), firstName.Trim(), lastName.Trim(), DateTimeOffset.UtcNow);
    }
}
