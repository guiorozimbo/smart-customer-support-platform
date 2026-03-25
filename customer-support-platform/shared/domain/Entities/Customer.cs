namespace CustomerSupportPlatform.Domain.Entities;

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? DocumentNumber { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<SupportTicket> Tickets { get; set; } = new List<SupportTicket>();
}
