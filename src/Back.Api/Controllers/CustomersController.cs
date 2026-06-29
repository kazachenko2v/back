using Back.Application.Customers;
using Microsoft.AspNetCore.Mvc;

namespace Back.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CustomersController(ICustomerService customers) : ControllerBase
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 200;

    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerDto>>> List(
        [FromQuery] int page = DefaultPage,
        [FromQuery] int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            return BadRequest(new { error = "Page must be greater than zero." });
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            return BadRequest(new { error = $"Page size must be between 1 and {MaxPageSize}." });
        }

        if (page > int.MaxValue / pageSize)
        {
            return BadRequest(new { error = "Page is too large." });
        }

        var result = await customers.ListAsync(page, pageSize, cancellationToken);
        Response.Headers["X-Cache"] = result.FromCache ? "HIT" : "MISS";

        return Ok(result.Page);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return NotFound();
        }

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await customers.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPatch("{id:guid}/email")]
    public async Task<ActionResult<CustomerDto>> ChangeEmail(
        Guid id,
        [FromBody] ChangeCustomerEmailRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await customers.ChangeEmailAsync(id, request, cancellationToken);
        if (customer is null)
        {
            return NotFound();
        }

        return Ok(customer);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await customers.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
