namespace CustomerSupportPlatform.Application.DTOs;

public record OrderDto(
    Guid Id,
    string OrderNumber,
    decimal TotalAmount,
    string Status,
    Guid CustomerId,
    string? Description,
    DateTime CreatedAt);

public record CreateOrderRequest(string OrderNumber, decimal TotalAmount, Guid CustomerId, string? Description);
public record UpdateOrderRequest(string? Status, string? Description);
