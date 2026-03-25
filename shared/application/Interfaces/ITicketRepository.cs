using CustomerSupportPlatform.Domain.Entities;

namespace CustomerSupportPlatform.Application.Interfaces;

public interface ITicketRepository : IRepository<SupportTicket>
{
    Task<IReadOnlyList<SupportTicket>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SupportTicket>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
}
