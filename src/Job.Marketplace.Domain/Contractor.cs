namespace Job.Marketplace.Domain;

public sealed class Contractor
{
    public Guid Id { get; }
    public string Name { get; }
    public decimal Rating { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    private Contractor(Guid id, string name, decimal rating, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        Rating = rating;
        CreatedAt = createdAt;
    }

    public static Contractor Create(string name, decimal rating = 0m)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (rating < 0 || rating > 5)
            throw new ArgumentException("Rating must be between 0 and 5.", nameof(rating));
        return new Contractor(Guid.NewGuid(), name.Trim(), rating, DateTimeOffset.UtcNow);
    }

    public void UpdateRating(decimal rating)
    {
        if (rating < 0 || rating > 5)
            throw new ArgumentException("Rating must be between 0 and 5.", nameof(rating));
        Rating = rating;
    }
}
