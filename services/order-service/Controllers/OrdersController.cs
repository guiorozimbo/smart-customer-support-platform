using CustomerSupportPlatform.Application.DTOs;
using CustomerSupportPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderAppService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderAppService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetAll(CancellationToken ct) =>
        Ok(await _orderService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken ct)
    {
        var order = await _orderService.GetByIdAsync(id, ct);
        return order == null ? NotFound() : Ok(order);
    }

    [HttpGet("number/{orderNumber}")]
    public async Task<ActionResult<OrderDto>> GetByOrderNumber(string orderNumber, CancellationToken ct)
    {
        var order = await _orderService.GetByOrderNumberAsync(orderNumber, ct);
        return order == null ? NotFound() : Ok(order);
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetByCustomer(Guid customerId, CancellationToken ct) =>
        Ok(await _orderService.GetByCustomerIdAsync(customerId, ct));

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var order = await _orderService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrderDto>> Update(Guid id, [FromBody] UpdateOrderRequest request, CancellationToken ct)
    {
        var order = await _orderService.UpdateAsync(id, request, ct);
        return order == null ? NotFound() : Ok(order);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct) =>
        await _orderService.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
