using Back.Domain.Common;

namespace Back.Domain.Customers;

public sealed class Customer : AggregateRoot
{
    private Customer()
    {
    }

    private Customer(Guid id, string name, EmailAddress email, DateTime createdAtUtc)
        : base(id)
    {
        Name = name;
        Email = email;
        CreatedAtUtc = createdAtUtc;
    }

    public string Name { get; private set; } = string.Empty;

    public EmailAddress Email { get; private set; } = null!;

    public DateTime CreatedAtUtc { get; private set; }

    public static Customer Create(string name, string email)
    {
        var customer = new Customer(
            Guid.NewGuid(),
            NormalizeName(name),
            EmailAddress.Create(email),
            DateTime.UtcNow);

        customer.AddDomainEvent(new CustomerCreatedDomainEvent(customer.Id));

        return customer;
    }

    public void Rename(string name)
    {
        Name = NormalizeName(name);
    }

    public void ChangeEmail(string email)
    {
        Email = EmailAddress.Create(email);
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Customer name is required.", nameof(name));
        }

        var normalized = name.Trim();
        if (normalized.Length > 200)
        {
            throw new ArgumentException("Customer name cannot exceed 200 characters.", nameof(name));
        }

        return normalized;
    }
}
