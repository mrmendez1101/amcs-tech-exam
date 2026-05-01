namespace Job.Marketplace.API.Features.Jobs.Delete;

public sealed class DeleteJobHandler(IDeleteJobQueries queries)
{
    public async Task HandleAsync(Guid id, CancellationToken ct)
    {
        if (!await queries.JobExistsAsync(id, ct))
            throw new KeyNotFoundException($"Job '{id}' not found.");

        await queries.DeleteAsync(id, ct);
    }
}
