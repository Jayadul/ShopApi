using System.Security.Claims;

namespace Shop.Api.Host.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(value, out var id)) return id;
        throw new InvalidOperationException("User ID claim not found.");
    }

    public static string GetUserName(this ClaimsPrincipal user)
    {
        var name = user.FindFirst(ClaimTypes.Name)?.Value;
        if (!string.IsNullOrEmpty(name)) return name;
        throw new InvalidOperationException("Username claim not found.");
    }

    public static string GetUserRole(this ClaimsPrincipal user)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (!string.IsNullOrEmpty(role)) return role;
        throw new InvalidOperationException("Role claim not found.");
    }
}
