using CustomerSupportPlatform.Domain.Entities;
using CustomerSupportPlatform.Application.Interfaces;
using Database;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupportPlatform.Infrastructure.Repositories;

public class TicketRepository : RepositoryBase<SupportTicket>, ITicketRepository
{
    public TicketRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<SupportTicket>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        await Context.SupportTickets
            .Include(t => t.Customer)
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SupportTicket>> GetByStatusAsync(string status, CancellationToken cancellationToken = default) =>
        await Context.SupportTickets
            .Include(t => t.Customer)
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
}
