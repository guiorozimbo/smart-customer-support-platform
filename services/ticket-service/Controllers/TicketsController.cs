using CustomerSupportPlatform.Application.DTOs;
using CustomerSupportPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TicketService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketAppService _ticketService;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(ITicketAppService ticketService, ILogger<TicketsController> logger)
    {
        _ticketService = ticketService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketDto>>> GetAll(CancellationToken ct) =>
        Ok(await _ticketService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketDto>> GetById(Guid id, CancellationToken ct)
    {
        var ticket = await _ticketService.GetByIdAsync(id, ct);
        return ticket == null ? NotFound() : Ok(ticket);
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IReadOnlyList<TicketDto>>> GetByCustomer(Guid customerId, CancellationToken ct) =>
        Ok(await _ticketService.GetByCustomerIdAsync(customerId, ct));

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IReadOnlyList<TicketDto>>> GetByStatus(string status, CancellationToken ct) =>
        Ok(await _ticketService.GetByStatusAsync(status, ct));

    [HttpPost]
    public async Task<ActionResult<TicketDto>> Create([FromBody] CreateTicketRequest request, CancellationToken ct)
    {
        var ticket = await _ticketService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TicketDto>> Update(Guid id, [FromBody] UpdateTicketRequest request, CancellationToken ct)
    {
        var ticket = await _ticketService.UpdateAsync(id, request, ct);
        return ticket == null ? NotFound() : Ok(ticket);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct) =>
        await _ticketService.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
