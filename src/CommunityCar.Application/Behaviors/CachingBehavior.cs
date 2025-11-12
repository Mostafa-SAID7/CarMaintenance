using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CommunityCar.Application.Behaviors;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly ICacheService _cacheService;

    public CachingBehavior(
        IDistributedCache cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger,
        ICacheService cacheService)
    {
        _cache = cache;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only cache query requests, not commands
        if (!(request is ICacheableQuery cacheableQuery))
        {
            return await next();
        }

        var cacheKey = cacheableQuery.CacheKey;
        var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey);

        if (cachedResponse != null)
        {
            _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
            return cachedResponse;
        }

        _logger.LogInformation("Cache miss for {CacheKey}, executing request", cacheKey);
        var response = await next();

        if (response != null)
        {
            await _cacheService.SetAsync(cacheKey, response, cacheableQuery.CacheDuration);
            _logger.LogInformation("Cached response for {CacheKey} with duration {Duration}",
                cacheKey, cacheableQuery.CacheDuration);
        }

        return response;
    }
}

public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan CacheDuration { get; }
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task RemoveByPatternAsync(string pattern);
}

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var cachedValue = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedValue))
                return default;

            return JsonSerializer.Deserialize<T>(cachedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached value for key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }

            var serializedValue = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serializedValue, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cached value for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached value for key {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var value = await _cache.GetAsync(key);
            return value != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if cached value exists for key {Key}", key);
            return false;
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        // Note: IDistributedCache doesn't support pattern matching
        // This is a simplified implementation
        // In production, you might want to use Redis-specific features or maintain a cache key registry
        _logger.LogWarning("RemoveByPatternAsync is not fully implemented for IDistributedCache. Pattern: {Pattern}", pattern);
    }
}

public static class CacheKeyGenerator
{
    public static string GenerateKey<T>(string prefix, object? parameters = null)
    {
        var key = $"{typeof(T).Name}:{prefix}";
        if (parameters != null)
        {
            var paramString = JsonSerializer.Serialize(parameters);
            key += $":{paramString.GetHashCode()}";
        }
        return key;
    }

    public static string GenerateUserSpecificKey<T>(string userId, string prefix, object? parameters = null)
    {
        return GenerateKey<T>($"user:{userId}:{prefix}", parameters);
    }

    public static string GenerateEntityKey<T>(object entityId, string prefix = "")
    {
        return $"{typeof(T).Name}:{entityId}:{prefix}".TrimEnd(':');
    }

    public static string GenerateListKey<T>(string prefix, object? filter = null)
    {
        var key = $"{typeof(T).Name}:list:{prefix}";
        if (filter != null)
        {
            key += $":{JsonSerializer.Serialize(filter).GetHashCode()}";
        }
        return key;
    }
}

public class CacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheInvalidationBehavior<TRequest, TResponse>> _logger;

    public CacheInvalidationBehavior(
        ICacheService cacheService,
        ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        if (request is ICacheInvalidationCommand invalidationCommand)
        {
            foreach (var cacheKey in invalidationCommand.CacheKeysToInvalidate)
            {
                await _cacheService.RemoveAsync(cacheKey);
                _logger.LogInformation("Invalidated cache key: {CacheKey}", cacheKey);
            }

            if (!string.IsNullOrEmpty(invalidationCommand.CacheKeyPattern))
            {
                await _cacheService.RemoveByPatternAsync(invalidationCommand.CacheKeyPattern);
                _logger.LogInformation("Invalidated cache pattern: {Pattern}", invalidationCommand.CacheKeyPattern);
            }
        }

        return response;
    }
}

public interface ICacheInvalidationCommand
{
    IEnumerable<string> CacheKeysToInvalidate { get; }
    string? CacheKeyPattern { get; }
}
