using CustomerSupportPlatform.Application.DTOs;

namespace CustomerSupportPlatform.Application.Interfaces;

/// <summary>
/// Application service for support ticket operations.
/// </summary>
public interface ITicketAppService
{
    Task<TicketDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TicketDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TicketDto>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TicketDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TicketDto> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken = default);
    Task<TicketDto?> UpdateAsync(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
