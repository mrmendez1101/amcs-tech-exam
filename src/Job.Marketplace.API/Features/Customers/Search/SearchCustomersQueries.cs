namespace Job.Marketplace.API.Features.Customers.Search;

public interface ISearchCustomersQueries
{
    Task<(IReadOnlyList<CustomerSummary> Items, int Total)> SearchAsync(
        string? term, int page, int pageSize, CancellationToken ct);
}

public sealed class SearchCustomersQueries(IDbConnectionFactory factory) : ISearchCustomersQueries
{
    private const string CountSql = """
        SELECT COUNT(*) FROM customers
        WHERE @term IS NULL
           OR last_name ILIKE @term || '%'
           OR id::text = @term
        """;

    private const string PageSql = """
        SELECT id, first_name, last_name, created_at
        FROM customers
        WHERE @term IS NULL
           OR last_name ILIKE @term || '%'
           OR id::text = @term
        ORDER BY last_name, first_name
        LIMIT @take OFFSET @skip
        """;

    public async Task<(IReadOnlyList<CustomerSummary>, int)> SearchAsync(
        string? term, int page, int pageSize, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        var normalised = string.IsNullOrWhiteSpace(term) ? null : term.Trim();
        var args = new { term = normalised, take = pageSize, skip = (page - 1) * pageSize };

        var total = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(CountSql, args, cancellationToken: ct));

        var items = (await conn.QueryAsync<CustomerSummary>(
            new CommandDefinition(PageSql, args, cancellationToken: ct))).ToList();

        return (items, total);
    }
}
