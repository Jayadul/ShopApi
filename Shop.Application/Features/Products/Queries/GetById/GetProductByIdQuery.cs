using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Application.Features.Products.Queries.GetById;

public class GetProductByIdQuery : IRequest<IActionResult>
{
    public int Id { get; set; }
}
