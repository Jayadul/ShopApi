namespace Shop.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }  // captured at time of order
    public decimal LineTotal => UnitPrice * Quantity;

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
