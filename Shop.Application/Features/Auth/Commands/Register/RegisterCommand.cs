using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Application.Features.Auth.Commands.Register;

public class RegisterCommand : IRequest<IActionResult>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
