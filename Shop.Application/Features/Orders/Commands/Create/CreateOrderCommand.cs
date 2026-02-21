using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Features.Orders.DTOs;

namespace Shop.Application.Features.Orders.Commands.Create;

public class CreateOrderCommand : IRequest<IActionResult>
{
    public int CustomerId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
}
