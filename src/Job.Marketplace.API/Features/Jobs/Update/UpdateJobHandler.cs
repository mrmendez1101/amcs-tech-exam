namespace Job.Marketplace.API.Features.Jobs.Update;

public sealed class UpdateJobHandler(IUpdateJobQueries queries)
{
    public async Task<UpdateJobResponse> HandleAsync(UpdateJobCommand cmd, CancellationToken ct)
    {
        var status = await queries.GetJobStatusAsync(cmd.Id, ct);
        if (status is null)
            throw new KeyNotFoundException($"Job '{cmd.Id}' not found.");
        if (status != "Open")
            throw new InvalidOperationException("Only open jobs can be updated.");

        await queries.UpdateAsync(cmd, ct);
        return new UpdateJobResponse(cmd.Id);
    }
}
