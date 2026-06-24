namespace Back.Domain.Customers;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Customer>> ListPageAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    void Add(Customer customer);

    void Remove(Customer customer);
}
