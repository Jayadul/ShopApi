using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Api.Host.Extensions;
using Shop.Application.Features.Orders.Commands.Create;
using Shop.Application.Features.Orders.Queries.GetAll;

namespace Shop.Api.Host.Controllers;

[Authorize]
public class OrdersController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllOrdersQuery query)
    {
        query.UserId = User.GetUserId();
        query.UserRole = User.GetUserRole();
        return await Mediator.Send(query);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
    {
        command.CreatedBy = User.GetUserId();
        return await Mediator.Send(command);
    }
}
