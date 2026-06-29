using Back.Application.Customers;

namespace Back.Infrastructure.Caching;

internal sealed class NullCustomerPageCache : ICustomerPageCache
{
    public Task<PagedResult<CustomerDto>?> GetAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<PagedResult<CustomerDto>?>(null);
    }

    public Task SetAsync(
        PagedResult<CustomerDto> page,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
