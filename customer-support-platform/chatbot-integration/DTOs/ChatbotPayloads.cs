namespace ChatbotIntegration.DTOs;

/// <summary>
/// Simplified DTOs for chatbot (e.g. Blip) consumption - flat and easy to parse.
/// </summary>
public record ChatbotCustomerResponse(
    string Id,
    string Name,
    string Email,
    string? Phone,
    string? DocumentNumber);

/// <summary>Resposta mínima quando o canal só precisa de confirmar o email.</summary>
public record ChatbotCustomerEmailOnlyResponse(string Email);

public record ChatbotOrderResponse(
    string Id,
    string OrderNumber,
    decimal TotalAmount,
    string Status,
    string? Description,
    string CreatedAt);

public record ChatbotTicketResponse(
    string Id,
    string Title,
    string Status,
    string Priority,
    string CreatedAt);

public record CreateTicketFromChatRequest(
    string CustomerEmail,
    string Title,
    string Description,
    string? OrderNumber = null,
    string? Priority = "Medium");

public record ChatbotTicketCreatedResponse(string TicketId, string Message);
