using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Application.Features.Customers.Commands.Create;

public class CreateCustomerCommand : IRequest<IActionResult>
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int? CreatedBy { get; set; }
}
