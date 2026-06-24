using Back.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace Back.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository(AppDbContext dbContext) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Customers
            .SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Customers
            .AsNoTracking()
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> ListPageAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.Name)
            .ThenBy(customer => customer.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public void Add(Customer customer)
    {
        dbContext.Customers.Add(customer);
    }

    public void Remove(Customer customer)
    {
        dbContext.Customers.Remove(customer);
    }
}
