using ChatbotIntegration.DTOs;
using CustomerSupportPlatform.Application.Interfaces;
using CustomerSupportPlatform.Domain.Constants;
using CustomerSupportPlatform.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotIntegration.Controllers;

/// <summary>
/// REST API for chatbot platforms (e.g. Blip). Endpoints are designed for server-side chatbot integration.
/// </summary>
[ApiController]
[Route("api/chatbot")]
public class ChatbotController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<ChatbotController> _logger;

    public ChatbotController(
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository,
        ITicketRepository ticketRepository,
        ILogger<ChatbotController> logger)
    {
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _ticketRepository = ticketRepository;
        _logger = logger;
    }

    /// <summary>Get customer by email (e.g. to identify user from Blip).</summary>
    [HttpGet("customer")]
    public async Task<ActionResult<ChatbotCustomerResponse>> GetCustomerByEmail(
        [FromQuery] string email,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("email is required");
        var customer = await _customerRepository.GetByEmailAsync(email.Trim(), ct);
        if (customer == null)
            return NotFound();
        return Ok(new ChatbotCustomerResponse(
            customer.Id.ToString(),
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.DocumentNumber));
    }

    /// <summary>Confirma o cliente pelo email e devolve só o email (payload mínimo para chat / demo).</summary>
    [HttpGet("customer/email")]
    public async Task<ActionResult<ChatbotCustomerEmailOnlyResponse>> GetCustomerEmailOnly(
        [FromQuery] string email,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("email is required");
        var customer = await _customerRepository.GetByEmailAsync(email.Trim(), ct);
        if (customer == null)
            return NotFound();
        return Ok(new ChatbotCustomerEmailOnlyResponse(customer.Email));
    }

    /// <summary>Get orders by customer email.</summary>
    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyList<ChatbotOrderResponse>>> GetOrdersByCustomerEmail(
        [FromQuery] string email,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("email is required");
        var customer = await _customerRepository.GetByEmailAsync(email.Trim(), ct);
        if (customer == null)
            return NotFound();
        var orders = await _orderRepository.GetByCustomerIdAsync(customer.Id, ct);
        var list = orders.Select(o => new ChatbotOrderResponse(
            o.Id.ToString(),
            o.OrderNumber,
            o.TotalAmount,
            o.Status,
            o.Description,
            o.CreatedAt.ToString("O"))).ToList();
        return Ok(list);
    }

    /// <summary>Get single order by order number (e.g. "ORD-12345").</summary>
    [HttpGet("order/{orderNumber}")]
    public async Task<ActionResult<ChatbotOrderResponse>> GetOrderByNumber(string orderNumber, CancellationToken ct)
    {
        var order = await _orderRepository.GetByOrderNumberAsync(orderNumber, ct);
        if (order == null)
            return NotFound();
        return Ok(new ChatbotOrderResponse(
            order.Id.ToString(),
            order.OrderNumber,
            order.TotalAmount,
            order.Status,
            order.Description,
            order.CreatedAt.ToString("O")));
    }

    /// <summary>Get open tickets by customer email.</summary>
    [HttpGet("tickets")]
    public async Task<ActionResult<IReadOnlyList<ChatbotTicketResponse>>> GetTicketsByEmail(
        [FromQuery] string email,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("email is required");
        var customer = await _customerRepository.GetByEmailAsync(email.Trim(), ct);
        if (customer == null)
            return NotFound();
        var tickets = await _ticketRepository.GetByCustomerIdAsync(customer.Id, ct);
        var list = tickets.Select(t => new ChatbotTicketResponse(
            t.Id.ToString(),
            t.Title,
            t.Status,
            t.Priority,
            t.CreatedAt.ToString("O"))).ToList();
        return Ok(list);
    }

    /// <summary>Create a support ticket from the chatbot (e.g. user reported an issue in Blip).</summary>
    [HttpPost("ticket")]
    public async Task<ActionResult<ChatbotTicketCreatedResponse>> CreateTicket(
        [FromBody] CreateTicketFromChatRequest? request,
        CancellationToken ct)
    {
        if (request is null)
            return BadRequest("Request body is required");
        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
            return BadRequest("CustomerEmail is required");
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title is required");
        if (string.IsNullOrWhiteSpace(request.Description))
            return BadRequest("Description is required");

        var customer = await _customerRepository.GetByEmailAsync(request.CustomerEmail.Trim(), ct);
        if (customer == null)
            return NotFound($"Customer not found for email: {request.CustomerEmail}");

        Guid? orderId = null;
        if (!string.IsNullOrWhiteSpace(request.OrderNumber))
        {
            var order = await _orderRepository.GetByOrderNumberAsync(request.OrderNumber.Trim(), ct);
            orderId = order?.Id;
        }

        var ticket = new SupportTicket
        {
            Title = request.Title,
            Description = request.Description,
            CustomerId = customer.Id,
            OrderId = orderId,
            Priority = request.Priority ?? TicketPriority.Medium,
            Status = TicketStatus.Open
        };
        var created = await _ticketRepository.AddAsync(ticket, ct);
        _logger.LogInformation("Chatbot created ticket {TicketId} for customer {Email}", created.Id, request.CustomerEmail);

        return Ok(new ChatbotTicketCreatedResponse(
            created.Id.ToString(),
            $"Ticket #{created.Id.ToString("N")[..8]} created. Our team will get back to you soon."));
    }
}
