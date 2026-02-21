using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Common.Models;

namespace Shop.Application.Features.Customers.Queries.GetAll;

public class GetAllCustomersQuery : PaginationParams, IRequest<IActionResult>
{
}
