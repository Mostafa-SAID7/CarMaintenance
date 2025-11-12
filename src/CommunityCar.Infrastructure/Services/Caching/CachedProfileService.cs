using CommunityCar.Application.DTOs.Auth.Profile;
using CommunityCar.Application.Interfaces.Auth;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services;

public class CachedProfileService : ICachedProfileService
{
    private readonly IProfileService _innerService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedProfileService> _logger;

    private const string ProfileCacheKeyPrefix = "profile:";
    private const string PublicProfileCacheKeyPrefix = "public_profile:";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

    public CachedProfileService(
        IProfileService innerService,
        ICacheService cacheService,
        ILogger<CachedProfileService> logger)
    {
        _innerService = innerService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<UserProfileDto?> GetProfileAsync(string userId)
    {
        var cacheKey = $"{ProfileCacheKeyPrefix}{userId}";

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                _logger.LogDebug("Fetching profile from service for user {UserId}", userId);
                return await _innerService.GetProfileAsync(userId);
            },
            _cacheExpiration);
    }

    public async Task<UserProfileDto?> GetPublicProfileAsync(string userId)
    {
        var cacheKey = $"{PublicProfileCacheKeyPrefix}{userId}";

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                _logger.LogDebug("Fetching public profile from service for user {UserId}", userId);
                return await _innerService.GetPublicProfileAsync(userId);
            },
            _cacheExpiration);
    }

    public async Task<UserProfileDto> CreateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var result = await _innerService.CreateProfileAsync(userId, request);

        // Invalidate any existing cache for this user
        await InvalidateProfileCacheAsync(userId);

        _logger.LogInformation("Profile created and cache invalidated for user {UserId}", userId);
        return result;
    }

    public async Task<UserProfileDto?> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var result = await _innerService.UpdateProfileAsync(userId, request);

        if (result != null)
        {
            // Invalidate cache after successful update
            await InvalidateProfileCacheAsync(userId);
            _logger.LogInformation("Profile updated and cache invalidated for user {UserId}", userId);
        }

        return result;
    }

    public async Task<bool> DeleteProfileAsync(string userId)
    {
        var result = await _innerService.DeleteProfileAsync(userId);

        if (result)
        {
            // Invalidate cache after successful deletion
            await InvalidateProfileCacheAsync(userId);
            _logger.LogInformation("Profile deleted and cache invalidated for user {UserId}", userId);
        }

        return result;
    }

    public async Task<string?> UploadProfilePictureAsync(string userId, IFormFile file)
    {
        var result = await _innerService.UploadProfilePictureAsync(userId, file);

        if (!string.IsNullOrEmpty(result))
        {
            // Invalidate cache after successful upload
            await InvalidateProfileCacheAsync(userId);
            _logger.LogInformation("Profile picture uploaded and cache invalidated for user {UserId}", userId);
        }

        return result;
    }

    public async Task<string?> UploadCoverPhotoAsync(string userId, IFormFile file)
    {
        var result = await _innerService.UploadCoverPhotoAsync(userId, file);

        if (!string.IsNullOrEmpty(result))
        {
            // Invalidate cache after successful upload
            await InvalidateProfileCacheAsync(userId);
            _logger.LogInformation("Cover photo uploaded and cache invalidated for user {UserId}", userId);
        }

        return result;
    }

    public async Task<bool> DeleteProfilePictureAsync(string userId)
    {
        var result = await _innerService.DeleteProfilePictureAsync(userId);

        if (result)
        {
            // Invalidate cache after successful deletion
            await InvalidateProfileCacheAsync(userId);
            _logger.LogInformation("Profile picture deleted and cache invalidated for user {UserId}", userId);
        }

        return result;
    }

    public async Task<bool> DeleteCoverPhotoAsync(string userId)
    {
        var result = await _innerService.DeleteCoverPhotoAsync(userId);

        if (result)
        {
            // Invalidate cache after successful deletion
            await InvalidateProfileCacheAsync(userId);
            _logger.LogInformation("Cover photo deleted and cache invalidated for user {UserId}", userId);
        }

        return result;
    }

    public async Task<bool> ValidateProfileDataAsync(UpdateProfileRequest request)
    {
        // Validation doesn't need caching as it's typically fast and depends on input
        return await _innerService.ValidateProfileDataAsync(request);
    }

    public async Task InvalidateProfileCacheAsync(string userId)
    {
        var profileKey = $"{ProfileCacheKeyPrefix}{userId}";
        var publicProfileKey = $"{PublicProfileCacheKeyPrefix}{userId}";

        await _cacheService.RemoveAsync(profileKey);
        await _cacheService.RemoveAsync(publicProfileKey);

        _logger.LogDebug("Cache invalidated for user {UserId}", userId);
    }

    public async Task InvalidateAllCacheAsync()
    {
        // Note: This is a simplified implementation
        // In a real-world scenario, you might want to use cache tags or patterns
        _logger.LogWarning("InvalidateAllCacheAsync called - this is a no-op in current implementation");
    }
}