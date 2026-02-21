using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Application.Features.Products.Commands.Delete;

public class DeleteProductCommand : IRequest<IActionResult>
{
    public int Id { get; set; }
}
