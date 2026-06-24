using Back.Domain.Common;

namespace Back.Domain.Customers;

public sealed record CustomerCreatedDomainEvent(Guid CustomerId) : DomainEvent(DateTime.UtcNow);
