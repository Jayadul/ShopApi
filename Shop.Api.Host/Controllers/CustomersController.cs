using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Api.Host.Extensions;
using Shop.Application.Features.Customers.Commands.Create;
using Shop.Application.Features.Customers.Commands.Delete;
using Shop.Application.Features.Customers.Commands.Update;
using Shop.Application.Features.Customers.Queries.GetAll;
using Shop.Application.Features.Customers.Queries.GetById;

namespace Shop.Api.Host.Controllers;

[Authorize]
public class CustomersController : BaseApiController
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] GetAllCustomersQuery query)
        => await Mediator.Send(query);

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById([FromRoute] int id)
        => await Mediator.Send(new GetCustomerByIdQuery { Id = id });

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
    {
        command.CreatedBy = User.GetUserId();
        return await Mediator.Send(command);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCustomerCommand command)
    {
        command.Id = id;
        command.UpdatedBy = User.GetUserId();
        return await Mediator.Send(command);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] int id)
        => await Mediator.Send(new DeleteCustomerCommand { Id = id });
}
