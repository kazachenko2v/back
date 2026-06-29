namespace Back.Application.Customers;

public interface ICustomerPageCache
{
    Task<PagedResult<CustomerDto>?> GetAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task SetAsync(
        PagedResult<CustomerDto> page,
        CancellationToken cancellationToken = default);

    Task InvalidateAsync(CancellationToken cancellationToken = default);
}
