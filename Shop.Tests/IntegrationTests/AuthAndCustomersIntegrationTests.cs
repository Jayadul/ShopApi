using FluentAssertions;
using Shop.Application.Common.Models;
using Shop.Application.Features.Auth.Commands.Login;
using Shop.Application.Features.Auth.Commands.Register;
using Shop.Application.Features.Auth.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Shop.Tests.IntegrationTests;

public class AuthAndCustomersIntegrationTests : IClassFixture<ShopWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthAndCustomersIntegrationTests(ShopWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var loginCommand = new LoginCommand
        {
            Email = "admin@shop.com",
            Password = "Admin123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponseDto>>();
        return result!.Data!.Token;
    }

    private async Task<string> GetCustomerTokenAsync()
    {
        var loginCommand = new LoginCommand
        {
            Email = "john@customer.com",
            Password = "Customer123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponseDto>>();
        return result!.Data!.Token;
    }

    [Fact]
    public async Task Register_ValidUser_ReturnsOk()
    {
        var uniqueEmail = $"test_{Guid.NewGuid()}@example.com";
        var command = new RegisterCommand
        {
            Username = "newuser",
            Email = uniqueEmail,
            Password = "Password123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetCustomers_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/customers");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}