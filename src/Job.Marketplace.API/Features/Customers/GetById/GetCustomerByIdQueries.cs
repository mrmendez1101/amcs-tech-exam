namespace Job.Marketplace.API.Features.Customers.GetById;

public interface IGetCustomerByIdQueries
{
    Task<CustomerDetail?> GetByIdAsync(Guid id, CancellationToken ct);
}

public sealed class GetCustomerByIdQueries(IDbConnectionFactory factory) : IGetCustomerByIdQueries
{
    private const string Sql = """
        SELECT id, first_name, last_name, created_at
        FROM customers WHERE id = @id
        """;

    public async Task<CustomerDetail?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<CustomerDetail>(
            new CommandDefinition(Sql, new { id }, cancellationToken: ct));
    }
}
