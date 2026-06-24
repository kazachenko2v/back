using Back.Application.Abstractions;
using Back.Domain.Customers;

namespace Back.Application.Customers;

internal sealed class CustomerService(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork) : ICustomerService
{
    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = Customer.Create(request.Name, request.Email);

        customers.Add(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(customer);
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await customers.GetByIdAsync(id, cancellationToken);
        return customer is null ? null : Map(customer);
    }

    public async Task<PagedResult<CustomerDto>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var skip = (page - 1) * pageSize;
        var totalCount = await customers.CountAsync(cancellationToken);
        var customerList = await customers.ListPageAsync(skip, pageSize, cancellationToken);

        return new PagedResult<CustomerDto>(
            customerList.Select(Map).ToList(),
            page,
            pageSize,
            totalCount);
    }

    public async Task<CustomerDto?> ChangeEmailAsync(
        Guid id,
        ChangeCustomerEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return null;
        }

        customer.ChangeEmail(request.Email);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(customer);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return false;
        }

        customers.Remove(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static CustomerDto Map(Customer customer)
    {
        return new CustomerDto(
            customer.Id,
            customer.Name,
            customer.Email.Value,
            customer.CreatedAtUtc);
    }
}
