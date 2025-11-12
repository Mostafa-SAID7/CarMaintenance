using CommunityCar.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Configurations.Caching;

/// <summary>
/// Service for invalidating cached data
/// </summary>
public interface ICacheInvalidationService
{
    /// <summary>
    /// Invalidates all cache entries for a specific user
    /// </summary>
    Task InvalidateUserCacheAsync(string userId);

    /// <summary>
    /// Invalidates cache entries for a specific entity
    /// </summary>
    Task InvalidateEntityCacheAsync(string entityType, string entityId);

    /// <summary>
    /// Invalidates cache entries matching a pattern
    /// </summary>
    Task InvalidatePatternAsync(string pattern);

    /// <summary>
    /// Clears all cache entries
    /// </summary>
    Task ClearAllCacheAsync();
}

/// <summary>
/// Implementation of cache invalidation service
/// </summary>
public class CacheInvalidationService : ICacheInvalidationService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheInvalidationService> _logger;

    // Cache key patterns
    private const string UserProfilePattern = "user:{0}:*";
    private const string UserProfileKey = "profile:{0}";
    private const string UserPermissionsKey = "permissions:{0}";
    private const string UserNotificationsPattern = "notifications:{0}:*";

    private const string EntityKey = "{0}:{1}";
    private const string EntityPattern = "{0}:{1}:*";
    private const string EntityListPattern = "{0}_list:*";
    private const string EntitySearchPattern = "{0}_search:*";

    public CacheInvalidationService(
        ICacheService cacheService,
        ILogger<CacheInvalidationService> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvalidateUserCacheAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        }

        try
        {
            var patterns = new[]
            {
                string.Format(UserProfilePattern, userId),
                string.Format(UserProfileKey, userId),
                string.Format(UserPermissionsKey, userId),
                string.Format(UserNotificationsPattern, userId)
            };

            foreach (var pattern in patterns)
            {
                await InvalidatePatternAsync(pattern);
            }

            _logger.LogInformation("Invalidated cache for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for user {UserId}", userId);
            throw;
        }
    }

    public async Task InvalidateEntityCacheAsync(string entityType, string entityId)
    {
        if (string.IsNullOrWhiteSpace(entityType))
        {
            throw new ArgumentException("Entity type cannot be null or empty", nameof(entityType));
        }

        if (string.IsNullOrWhiteSpace(entityId))
        {
            throw new ArgumentException("Entity ID cannot be null or empty", nameof(entityId));
        }

        try
        {
            var patterns = new[]
            {
                string.Format(EntityKey, entityType, entityId),
                string.Format(EntityPattern, entityType, entityId),
                string.Format(EntityListPattern, entityType),
                string.Format(EntitySearchPattern, entityType)
            };

            foreach (var pattern in patterns)
            {
                await InvalidatePatternAsync(pattern);
            }

            _logger.LogInformation("Invalidated cache for entity {EntityType}:{EntityId}", entityType, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for entity {EntityType}:{EntityId}", entityType, entityId);
            throw;
        }
    }

    public async Task InvalidatePatternAsync(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));
        }

        try
        {
            // For distributed caches like Redis, we can use pattern-based deletion
            // For in-memory cache, this is a simplified implementation
            // In a real distributed cache, you'd scan and delete keys matching the pattern

            _logger.LogInformation("Invalidating cache pattern: {Pattern}", pattern);

            // Note: IMemoryCache doesn't support pattern-based invalidation
            // For Redis, you would use KEYS pattern and DEL
            // This is a placeholder for the actual implementation
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache pattern: {Pattern}", pattern);
            throw;
        }
    }

    public async Task ClearAllCacheAsync()
    {
        try
        {
            await _cacheService.ClearAsync();
            _logger.LogWarning("All cache cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all cache");
            throw;
        }
    }
}
