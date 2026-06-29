using Back.Application.Abstractions;
using Back.Domain.Customers;

namespace Back.Application.Customers;

internal sealed class CustomerService(
    ICustomerRepository customers,
    ICustomerPageCache customerPageCache,
    IUnitOfWork unitOfWork) : ICustomerService
{
    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = Customer.Create(request.Name, request.Email);

        customers.Add(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await customerPageCache.InvalidateAsync(cancellationToken);

        return Map(customer);
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await customers.GetByIdAsync(id, cancellationToken);
        return customer is null ? null : Map(customer);
    }

    public async Task<CustomerListResult> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var cachedPage = await customerPageCache.GetAsync(page, pageSize, cancellationToken);
        if (cachedPage is not null)
        {
            return new CustomerListResult(cachedPage, FromCache: true);
        }

        var skip = (page - 1) * pageSize;
        var totalCount = await customers.CountAsync(cancellationToken);
        var customerList = await customers.ListPageAsync(skip, pageSize, cancellationToken);

        var result = new PagedResult<CustomerDto>(
            customerList.Select(Map).ToList(),
            page,
            pageSize,
            totalCount);

        await customerPageCache.SetAsync(result, cancellationToken);

        return new CustomerListResult(result, FromCache: false);
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
        await customerPageCache.InvalidateAsync(cancellationToken);

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
        await customerPageCache.InvalidateAsync(cancellationToken);

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
