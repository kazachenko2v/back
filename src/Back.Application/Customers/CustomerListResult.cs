namespace Back.Application.Customers;

public sealed record CustomerListResult(
    PagedResult<CustomerDto> Page,
    bool FromCache);
