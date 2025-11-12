namespace CommunityCar.Application.Interfaces;

public interface ICurrentUserService
{
    string? GetCurrentUserId();
    string? GetCurrentUserName();
    string? GetCurrentUserEmail();
    bool IsAuthenticated();
    bool IsInRole(string role);
    IEnumerable<string> GetCurrentUserRoles();
    bool HasPermission(string permission);
    IEnumerable<string> GetCurrentUserPermissions();
    string? GetCurrentUserTimeZone();
    string? GetCurrentUserCulture();
    Dictionary<string, object>? GetCurrentUserClaims();
}
