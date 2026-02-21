using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Domain.Entities;
using Shop.Domain.Enums;
using System.Security.Cryptography;
using System.Text;

namespace Shop.Application.Features.Auth.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, IActionResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RegisterHandler> _logger;

    public RegisterHandler(IUserRepository userRepository, ILogger<RegisterHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IActionResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existing != null)
                return new BadRequestObjectResult(ApiResponse<string>.Fail("Email already registered."));

            using var hmac = new HMACSHA512();
            var passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password)));
            var passwordSalt = Convert.ToBase64String(hmac.Key);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = UserRole.Customer,   // new users are always Customer by default
                CreationDate = DateTime.UtcNow
            };

            _userRepository.Add(user);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return new OkObjectResult(ApiResponse<bool>.Ok(true, "User registered successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return new ObjectResult(ApiResponse<string>.Fail("An error occurred.")) { StatusCode = 500 };
        }
    }
}
