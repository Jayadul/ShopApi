using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Api.Host.Extensions;
using Shop.Application.Features.Products.Commands.Create;
using Shop.Application.Features.Products.Commands.Delete;
using Shop.Application.Features.Products.Commands.Update;
using Shop.Application.Features.Products.Queries.GetAll;
using Shop.Application.Features.Products.Queries.GetById;

namespace Shop.Api.Host.Controllers;

[Authorize]
public class ProductsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllProductsQuery query)
        => await Mediator.Send(query);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
        => await Mediator.Send(new GetProductByIdQuery { Id = id });

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        command.CreatedBy = User.GetUserId();
        return await Mediator.Send(command);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateProductCommand command)
    {
        command.Id = id;
        command.UpdatedBy = User.GetUserId();
        return await Mediator.Send(command);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] int id)
        => await Mediator.Send(new DeleteProductCommand { Id = id });
}
