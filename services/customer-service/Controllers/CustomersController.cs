using CustomerSupportPlatform.Application.DTOs;
using CustomerSupportPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerAppService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerAppService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> GetAll(CancellationToken ct) =>
        Ok(await _customerService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id, CancellationToken ct)
    {
        var customer = await _customerService.GetByIdAsync(id, ct);
        return customer == null ? NotFound() : Ok(customer);
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<CustomerDto>> GetByEmail(string email, CancellationToken ct)
    {
        var customer = await _customerService.GetByEmailAsync(email, ct);
        return customer == null ? NotFound() : Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var customer = await _customerService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken ct)
    {
        var customer = await _customerService.UpdateAsync(id, request, ct);
        return customer == null ? NotFound() : Ok(customer);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct) =>
        await _customerService.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
