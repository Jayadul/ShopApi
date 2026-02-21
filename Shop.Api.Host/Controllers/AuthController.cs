using Microsoft.AspNetCore.Mvc;
using Shop.Application.Features.Auth.Commands.Login;
using Shop.Application.Features.Auth.Commands.Register;

namespace Shop.Api.Host.Controllers;

public class AuthController : BaseApiController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        => await Mediator.Send(command);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
        => await Mediator.Send(command);
}
