using Shop.Domain.Enums;

namespace Shop.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public decimal TotalAmount { get; set; }  // sum of all OrderItem line totals
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? Notes { get; set; }

    public Customer Customer { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
