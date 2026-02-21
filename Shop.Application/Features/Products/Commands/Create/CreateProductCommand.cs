using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Application.Features.Products.Commands.Create;

public class CreateProductCommand : IRequest<IActionResult>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int? CreatedBy { get; set; }
}
