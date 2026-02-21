using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<IActionResult>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
