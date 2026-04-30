using DomainJob = Job.Marketplace.Domain.Job;

namespace Job.Marketplace.API.Features.Jobs.Create;

public sealed class CreateJobHandler(ICreateJobQueries queries)
{
    public async Task<CreateJobResponse> HandleAsync(CreateJobRequest request, CancellationToken ct)
    {
        if (!await queries.CustomerExistsAsync(request.CustomerId, ct))
            throw new KeyNotFoundException($"Customer '{request.CustomerId}' not found.");

        var job = DomainJob.Create(request.CustomerId, request.StartDate, request.DueDate, request.Budget, request.Description);
        var id = await queries.InsertJobAsync(job, ct);
        return new CreateJobResponse(id);
    }
}
