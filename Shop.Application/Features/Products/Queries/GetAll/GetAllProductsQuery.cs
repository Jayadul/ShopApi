using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Common.Models;

namespace Shop.Application.Features.Products.Queries.GetAll;

public class GetAllProductsQuery : PaginationParams, IRequest<IActionResult>
{
}
