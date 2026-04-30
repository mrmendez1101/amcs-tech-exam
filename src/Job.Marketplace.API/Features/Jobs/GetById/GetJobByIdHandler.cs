namespace Job.Marketplace.API.Features.Jobs.GetById;

public sealed class GetJobByIdHandler(IGetJobByIdQueries queries)
{
    public async Task<JobDetail?> HandleAsync(Guid id, CancellationToken ct)
        => await queries.GetByIdAsync(id, ct);
}
