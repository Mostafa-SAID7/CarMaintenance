using System.Security.Claims;

namespace CommunityCar.Application.Helpers;

public static class AuthHelper
{
    public static string? GetCurrentUserId(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public static string? GetCurrentUserEmail(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value;
    }

    public static IEnumerable<string> GetCurrentUserRoles(ClaimsPrincipal user)
    {
        return user.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }

    public static bool IsAuthenticated(ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated ?? false;
    }

    public static bool HasRole(ClaimsPrincipal user, string role)
    {
        return user.IsInRole(role);
    }

    public static bool HasAnyRole(ClaimsPrincipal user, params string[] roles)
    {
        return roles.Any(role => user.IsInRole(role));
    }
}