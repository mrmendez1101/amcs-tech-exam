namespace Job.Marketplace.API.Features.Customers.GetById;

public sealed class GetCustomerByIdHandler(IGetCustomerByIdQueries queries)
{
    public async Task<CustomerDetail?> HandleAsync(Guid id, CancellationToken ct)
        => await queries.GetByIdAsync(id, ct);
}
