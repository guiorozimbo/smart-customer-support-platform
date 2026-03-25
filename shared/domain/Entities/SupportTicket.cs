using CustomerSupportPlatform.Domain.Constants;

namespace CustomerSupportPlatform.Domain.Entities;

public class SupportTicket : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = TicketStatus.Open;
    public string Priority { get; set; } = TicketPriority.Medium;
    public Guid CustomerId { get; set; }
    public Guid? OrderId { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual Order? Order { get; set; }
}
