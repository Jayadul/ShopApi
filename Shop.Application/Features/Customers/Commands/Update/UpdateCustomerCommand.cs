using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Application.Features.Customers.Commands.Update;

public class UpdateCustomerCommand : IRequest<IActionResult>
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int? UpdatedBy { get; set; }
}
