namespace Job.Marketplace.Domain;

public sealed class JobOffer
{
    public Guid Id { get; }
    public Guid JobId { get; }
    public Guid ContractorId { get; }
    public decimal Price { get; }
    public DateTimeOffset CreatedAt { get; }

    private JobOffer(Guid id, Guid jobId, Guid contractorId, decimal price, DateTimeOffset createdAt)
    {
        Id = id;
        JobId = jobId;
        ContractorId = contractorId;
        Price = price;
        CreatedAt = createdAt;
    }

    public static JobOffer Create(Guid jobId, Guid contractorId, decimal price)
    {
        if (jobId == Guid.Empty) throw new ArgumentException("Job ID is required.", nameof(jobId));
        if (contractorId == Guid.Empty) throw new ArgumentException("Contractor ID is required.", nameof(contractorId));
        if (price <= 0) throw new ArgumentException("Price must be greater than zero.", nameof(price));
        return new JobOffer(Guid.NewGuid(), jobId, contractorId, price, DateTimeOffset.UtcNow);
    }
}
