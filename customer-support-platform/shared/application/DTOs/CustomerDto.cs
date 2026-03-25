namespace CustomerSupportPlatform.Application.DTOs;

public record CustomerDto(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? DocumentNumber,
    DateTime CreatedAt);

public record CreateCustomerRequest(string Name, string Email, string? Phone, string? DocumentNumber);
public record UpdateCustomerRequest(string? Name, string? Email, string? Phone, string? DocumentNumber);
