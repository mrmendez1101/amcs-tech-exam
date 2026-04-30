namespace Job.Marketplace.API.Features.Contractors.Search;

public interface ISearchContractorsQueries
{
    Task<(IReadOnlyList<ContractorSummary> Items, int Total)> SearchAsync(
        string? term, int page, int pageSize, CancellationToken ct);
}

public sealed class SearchContractorsQueries(IDbConnectionFactory factory) : ISearchContractorsQueries
{
    private const string CountSql = """
        SELECT COUNT(*) FROM contractors
        WHERE @term IS NULL
           OR name ILIKE @term || '%'
           OR id::text = @term
        """;

    private const string PageSql = """
        SELECT id, name, rating, created_at
        FROM contractors
        WHERE @term IS NULL
           OR name ILIKE @term || '%'
           OR id::text = @term
        ORDER BY name
        LIMIT @take OFFSET @skip
        """;

    public async Task<(IReadOnlyList<ContractorSummary>, int)> SearchAsync(
        string? term, int page, int pageSize, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        var normalised = string.IsNullOrWhiteSpace(term) ? null : term.Trim();
        var args = new { term = normalised, take = pageSize, skip = (page - 1) * pageSize };

        var total = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(CountSql, args, cancellationToken: ct));

        var items = (await conn.QueryAsync<ContractorSummary>(
            new CommandDefinition(PageSql, args, cancellationToken: ct))).ToList();

        return (items, total);
    }
}
