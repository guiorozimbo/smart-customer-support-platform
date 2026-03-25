using CustomerSupportPlatform.Domain.Constants;

namespace CustomerSupportPlatform.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = OrderStatus.Pending;
    public Guid CustomerId { get; set; }
    public string? Description { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
