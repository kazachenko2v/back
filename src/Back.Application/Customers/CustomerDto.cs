namespace Back.Application.Customers;

public sealed record CustomerDto(
    Guid Id,
    string Name,
    string Email,
    DateTime CreatedAtUtc);
