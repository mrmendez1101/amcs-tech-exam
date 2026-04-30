namespace Job.Marketplace.API.Features.JobOffers.Create;

public interface ICreateJobOfferQueries
{
    Task<string?> GetJobStatusAsync(Guid jobId, CancellationToken ct);
    Task<bool> ContractorExistsAsync(Guid contractorId, CancellationToken ct);
    Task<Guid> InsertOfferAsync(JobOffer offer, CancellationToken ct);
}

public sealed class CreateJobOfferQueries(IDbConnectionFactory factory) : ICreateJobOfferQueries
{
    public async Task<string?> GetJobStatusAsync(Guid jobId, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        return await conn.ExecuteScalarAsync<string?>(
            new CommandDefinition("SELECT status FROM jobs WHERE id = @jobId", new { jobId }, cancellationToken: ct));
    }

    public async Task<bool> ContractorExistsAsync(Guid contractorId, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        return await conn.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                "SELECT EXISTS(SELECT 1 FROM contractors WHERE id = @contractorId)",
                new { contractorId }, cancellationToken: ct));
    }

    public async Task<Guid> InsertOfferAsync(JobOffer offer, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO job_offers (id, job_id, contractor_id, price, created_at)
            VALUES (@Id, @JobId, @ContractorId, @Price, @CreatedAt)
            RETURNING id
            """;
        await using var conn = await factory.CreateAsync(ct);
        return await conn.ExecuteScalarAsync<Guid>(
            new CommandDefinition(sql, new
            {
                offer.Id,
                offer.JobId,
                offer.ContractorId,
                offer.Price,
                offer.CreatedAt
            }, cancellationToken: ct));
    }
}
