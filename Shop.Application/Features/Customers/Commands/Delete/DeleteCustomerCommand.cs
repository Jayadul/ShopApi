using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Application.Features.Customers.Commands.Delete;

public class DeleteCustomerCommand : IRequest<IActionResult>
{
    public int Id { get; set; }
}
