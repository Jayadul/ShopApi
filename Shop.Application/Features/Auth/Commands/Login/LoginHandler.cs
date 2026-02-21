using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Application.Features.Auth.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace Shop.Application.Features.Auth.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, IActionResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(IUserRepository userRepository, ITokenService tokenService, ILogger<LoginHandler> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<IActionResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (user == null)
                return new ObjectResult(ApiResponse<string>.Fail("Invalid email or password."))
                {
                    StatusCode = 401
                };

            using var hmac = new HMACSHA512(Convert.FromBase64String(user.PasswordSalt));
            var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password)));

            if (computedHash != user.PasswordHash)
                return new ObjectResult(ApiResponse<string>.Fail("Invalid email or password."))
                {
                    StatusCode = 401
                };

            var token = _tokenService.GenerateToken(user);

            var response = new TokenResponseDto
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString()   // "Customer" or "Admin"
            };

            return new OkObjectResult(ApiResponse<TokenResponseDto>.Ok(response, "Login successful."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return new ObjectResult(ApiResponse<string>.Fail("An error occurred.")) { StatusCode = 500 };
        }
    }
}