namespace CustomerSupportPlatform.Application.DTOs;

public record TicketDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    Guid CustomerId,
    Guid? OrderId,
    DateTime CreatedAt);

public record CreateTicketRequest(string Title, string Description, Guid CustomerId, Guid? OrderId, string? Priority);
public record UpdateTicketRequest(string? Status, string? Priority);
