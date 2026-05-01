using DomainJob = Job.Marketplace.Domain.Job;

namespace Job.Marketplace.API.Features.Jobs.Create;

public interface ICreateJobQueries
{
    Task<bool> CustomerExistsAsync(Guid customerId, CancellationToken ct);
    Task<Guid> InsertJobAsync(DomainJob job, CancellationToken ct);
}

public sealed class CreateJobQueries(IDbConnectionFactory factory) : ICreateJobQueries
{
    public async Task<bool> CustomerExistsAsync(Guid customerId, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        return await conn.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                "SELECT EXISTS(SELECT 1 FROM customers WHERE id = @customerId)",
                new { customerId }, cancellationToken: ct));
    }

    public async Task<Guid> InsertJobAsync(DomainJob job, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO jobs (id, customer_id, start_date, due_date, budget, description, created_at)
            VALUES (@Id, @CustomerId, @StartDate, @DueDate, @Budget, @Description, @CreatedAt)
            RETURNING id
            """;
        await using var conn = await factory.CreateAsync(ct);
        return await conn.ExecuteScalarAsync<Guid>(
            new CommandDefinition(sql, new
            {
                job.Id,
                job.CustomerId,
                job.StartDate,
                job.DueDate,
                job.Budget,
                job.Description,
                job.CreatedAt
            }, cancellationToken: ct));
    }
}
