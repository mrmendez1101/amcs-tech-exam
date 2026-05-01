namespace Job.Marketplace.API.Features.Jobs.Search;

public interface ISearchJobsQueries
{
    Task<(IReadOnlyList<JobSummary> Items, int Total)> SearchAsync(
        string? term, int page, int pageSize, CancellationToken ct);
}

public sealed class SearchJobsQueries(IDbConnectionFactory factory) : ISearchJobsQueries
{
    private const string CountSql = """
        SELECT COUNT(*) FROM jobs
        WHERE @term IS NULL
           OR description ILIKE '%' || @term || '%'
           OR id::text = @term
        """;

    private const string PageSql = """
        SELECT id, customer_id, start_date, due_date, budget, description, accepted_offer_id, created_at
        FROM jobs
        WHERE @term IS NULL
           OR description ILIKE '%' || @term || '%'
           OR id::text = @term
        ORDER BY created_at DESC
        LIMIT @take OFFSET @skip
        """;

    public async Task<(IReadOnlyList<JobSummary>, int)> SearchAsync(
        string? term, int page, int pageSize, CancellationToken ct)
    {
        await using var conn = await factory.CreateAsync(ct);
        var normalised = string.IsNullOrWhiteSpace(term) ? null : term.Trim();
        var args = new { term = normalised, take = pageSize, skip = (page - 1) * pageSize };

        var total = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(CountSql, args, cancellationToken: ct));
        var items = (await conn.QueryAsync<JobSummary>(
            new CommandDefinition(PageSql, args, cancellationToken: ct))).ToList();

        return (items, total);
    }
}
