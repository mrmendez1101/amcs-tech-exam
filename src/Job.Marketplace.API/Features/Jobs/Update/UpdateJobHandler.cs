namespace Job.Marketplace.API.Features.Jobs.Update;

public sealed class UpdateJobHandler(IUpdateJobQueries queries)
{
    public async Task<UpdateJobResponse> HandleAsync(UpdateJobCommand cmd, CancellationToken ct)
    {
        if (!await queries.JobExistsAsync(cmd.Id, ct))
            throw new KeyNotFoundException($"Job '{cmd.Id}' not found.");

        await queries.UpdateAsync(cmd, ct);
        return new UpdateJobResponse(cmd.Id);
    }
}
