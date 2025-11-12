using CommunityCar.Application.DTOs.Auth.Profile;

namespace CommunityCar.Application.Interfaces.Auth;

public interface ICachedProfileService : IProfileService
{
    Task InvalidateProfileCacheAsync(string userId);
    Task InvalidateAllCacheAsync();
}