namespace Back.Application.Customers;

public interface ICustomerService
{
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);

    Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<CustomerDto>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<CustomerDto?> ChangeEmailAsync(Guid id, ChangeCustomerEmailRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
