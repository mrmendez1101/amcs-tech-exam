namespace Job.Marketplace.API.Features.Jobs.Delete;

public sealed class DeleteJobHandler(IDeleteJobQueries queries)
{
    public async Task HandleAsync(Guid id, CancellationToken ct)
    {
        var status = await queries.GetJobStatusAsync(id, ct);
        if (status is null)
            throw new KeyNotFoundException($"Job '{id}' not found.");
        if (status == "Awarded")
            throw new InvalidOperationException("Awarded jobs cannot be cancelled.");

        await queries.SoftDeleteAsync(id, ct);
    }
}
