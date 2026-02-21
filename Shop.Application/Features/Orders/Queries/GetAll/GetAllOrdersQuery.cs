using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Common.Models;
using Shop.Domain.Enums;

namespace Shop.Application.Features.Orders.Queries.GetAll;

public class GetAllOrdersQuery : PaginationParams, IRequest<IActionResult>
{
    public int? CustomerId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public OrderStatus? Status { get; set; }
    public int UserId { get; set; }
    public string UserRole { get; set; } = string.Empty;
}
