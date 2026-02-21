namespace Shop.Application.Features.Orders.DTOs;

public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
