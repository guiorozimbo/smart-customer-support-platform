using CustomerSupportPlatform.Application.DTOs;
using CustomerSupportPlatform.Application.Interfaces;
using CustomerSupportPlatform.Domain.Constants;
using CustomerSupportPlatform.Domain.Entities;

namespace TicketService.Services;

/// <summary>
/// Application service for support ticket operations. Delegates persistence to <see cref="ITicketRepository"/>.
/// </summary>
public class TicketAppService : ITicketAppService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<TicketAppService> _logger;

    public TicketAppService(ITicketRepository ticketRepository, ILogger<TicketAppService> logger)
    {
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TicketDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, ct);
        return ticket is null ? null : MapToDto(ticket);
    }

    public async Task<IReadOnlyList<TicketDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        var tickets = await _ticketRepository.GetByCustomerIdAsync(customerId, ct);
        return tickets.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<TicketDto>> GetByStatusAsync(string status, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(status))
            return Array.Empty<TicketDto>();

        var tickets = await _ticketRepository.GetByStatusAsync(status.Trim(), ct);
        return tickets.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<TicketDto>> GetAllAsync(CancellationToken ct = default)
    {
        var tickets = await _ticketRepository.GetAllAsync(ct);
        return tickets.Select(MapToDto).ToList();
    }

    public async Task<TicketDto> CreateAsync(CreateTicketRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var priority = string.IsNullOrWhiteSpace(request.Priority) ? TicketPriority.Medium : request.Priority.Trim();
        var ticket = new SupportTicket
        {
            Title = request.Title,
            Description = request.Description,
            CustomerId = request.CustomerId,
            OrderId = request.OrderId,
            Priority = priority,
            Status = TicketStatus.Open
        };
        var created = await _ticketRepository.AddAsync(ticket, ct);
        _logger.LogInformation("Ticket created: {Id}, Customer: {CustomerId}", created.Id, created.CustomerId);
        return MapToDto(created);
    }

    public async Task<TicketDto?> UpdateAsync(Guid id, UpdateTicketRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var ticket = await _ticketRepository.GetByIdAsync(id, ct);
        if (ticket is null)
            return null;

        if (request.Status is { } status)
            ticket.Status = status;
        if (request.Priority is { } prio)
            ticket.Priority = prio;

        await _ticketRepository.UpdateAsync(ticket, ct);
        _logger.LogInformation("Ticket updated: {Id}", id);
        return MapToDto(ticket);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, ct);
        if (ticket is null)
            return false;

        await _ticketRepository.DeleteAsync(ticket, ct);
        _logger.LogInformation("Ticket deleted: {Id}", id);
        return true;
    }

    private static TicketDto MapToDto(SupportTicket t) => new(
        t.Id, t.Title, t.Description, t.Status, t.Priority, t.CustomerId, t.OrderId, t.CreatedAt);
}
