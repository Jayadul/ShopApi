using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Application.Features.Customers.Queries.GetById;

public class GetCustomerByIdQuery : IRequest<IActionResult>
{
    public int Id { get; set; }
}
