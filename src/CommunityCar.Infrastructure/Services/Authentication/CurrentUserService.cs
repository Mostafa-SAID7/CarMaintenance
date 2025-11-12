using CommunityCar.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CommunityCar.Infrastructure.Services.Authentication;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public string? GetCurrentUserName()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name) ??
               _httpContextAccessor.HttpContext?.User?.FindFirstValue("preferred_username") ??
               _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }

    public string? GetCurrentUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
    }

    public IEnumerable<string> GetCurrentUserRoles()
    {
        return _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)
            .Select(c => c.Value) ?? Enumerable.Empty<string>();
    }

    public IEnumerable<string> GetCurrentUserPermissions()
    {
        // In a real implementation, you might have custom claims for permissions
        // For now, we'll derive permissions from roles
        var roles = GetCurrentUserRoles().ToList();

        var permissions = new List<string>();

        if (roles.Contains("Admin"))
        {
            permissions.AddRange(new[]
            {
                "users.manage", "content.manage", "system.admin", "reports.view",
                "settings.modify", "security.manage", "audit.view"
            });
        }

        if (roles.Contains("Moderator"))
        {
            permissions.AddRange(new[]
            {
                "content.moderate", "users.warn", "reports.view", "audit.view"
            });
        }

        if (roles.Contains("User") || roles.Any())
        {
            permissions.AddRange(new[]
            {
                "profile.manage", "content.create", "content.comment", "content.vote"
            });
        }

        return permissions.Distinct();
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }

    public bool HasPermission(string permission)
    {
        var permissions = GetCurrentUserPermissions();
        return permissions.Contains(permission);
    }

    public string? GetCurrentUserTimeZone()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirstValue("timezone") ?? "UTC";
    }

    public string? GetCurrentUserLanguage()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirstValue("locale") ??
               _httpContextAccessor.HttpContext?.Request?.Headers["Accept-Language"].FirstOrDefault()?.Split(',').FirstOrDefault() ??
               "en";
    }

    public string GetCurrentUserIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
    }

    public string GetCurrentUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "unknown";
    }

    public DateTime? GetCurrentUserLastLogin()
    {
        var lastLoginClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("last_login");
        if (DateTime.TryParse(lastLoginClaim, out var lastLogin))
        {
            return lastLogin;
        }
        return null;
    }

    public bool IsCurrentUser(string userId)
    {
        return GetCurrentUserId() == userId;
    }

    public bool CanAccessUserData(string targetUserId)
    {
        var currentUserId = GetCurrentUserId();

        // Users can always access their own data
        if (currentUserId == targetUserId)
            return true;

        // Admins can access anyone's data
        if (IsInRole("Admin"))
            return true;

        // Moderators can access user data for moderation purposes
        if (IsInRole("Moderator"))
            return true;

        return false;
    }

    public bool CanModifyUserData(string targetUserId)
    {
        var currentUserId = GetCurrentUserId();

        // Users can modify their own data
        if (currentUserId == targetUserId)
            return true;

        // Admins can modify anyone's data
        if (IsInRole("Admin"))
            return true;

        return false;
    }

    public bool CanDeleteUserData(string targetUserId)
    {
        // Only admins can delete user data
        return IsInRole("Admin");
    }

    public Dictionary<string, object> GetCurrentUserClaims()
    {
        var claims = _httpContextAccessor.HttpContext?.User?.Claims;
        if (claims == null)
            return new Dictionary<string, object>();

        return claims.ToDictionary(
            c => c.Type,
            c => (object)c.Value
        );
    }

    public string? GetClaimValue(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirstValue(claimType);
    }

    public IEnumerable<string> GetClaimValues(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User?.FindAll(claimType)
            .Select(c => c.Value) ?? Enumerable.Empty<string>();
    }
}
