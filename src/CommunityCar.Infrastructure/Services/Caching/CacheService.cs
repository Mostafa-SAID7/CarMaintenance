using CommunityCar.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CommunityCar.Infrastructure.Services.Caching;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;
    private readonly ConcurrentDictionary<string, CacheEntryStats> _cacheStats;

    public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _cacheStats = new ConcurrentDictionary<string, CacheEntryStats>();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            if (_memoryCache.TryGetValue(key, out T? value))
            {
                UpdateStats(key, true);
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return value;
            }

            UpdateStats(key, false);
            _logger.LogDebug("Cache miss for key: {Key}", key);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving value from cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
            };

            // Add callback for cleanup
            cacheEntryOptions.RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                _logger.LogDebug("Cache entry evicted. Key: {Key}, Reason: {Reason}", key, reason);
            });

            _memoryCache.Set(key, value, cacheEntryOptions);
            _logger.LogDebug("Cache set for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            _cacheStats.TryRemove(key, out _);
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return _memoryCache.TryGetValue(key, out _);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence in cache for key: {Key}", key);
            return false;
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cachedValue = await GetAsync<T>(key);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        var value = await factory();
        await SetAsync(key, value, expiration);
        return value;
    }

    public async Task SetMultipleAsync<T>(Dictionary<string, T> keyValuePairs, TimeSpan? expiration = null)
    {
        foreach (var kvp in keyValuePairs)
        {
            await SetAsync(kvp.Key, kvp.Value, expiration);
        }
    }

    public async Task<Dictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys)
    {
        var result = new Dictionary<string, T?>();
        foreach (var key in keys)
        {
            result[key] = await GetAsync<T>(key);
        }
        return result;
    }

    public async Task RemoveMultipleAsync(IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            await RemoveAsync(key);
        }
    }

    public async Task ClearAsync()
    {
        // Note: IMemoryCache doesn't have a clear method, so we can't actually clear all entries
        // This is a limitation of the in-memory cache implementation
        _logger.LogWarning("ClearAsync called but not implemented for IMemoryCache");
    }

    public async Task<CacheStatistics> GetStatisticsAsync()
    {
        var stats = new CacheStatistics
        {
            TotalEntries = _cacheStats.Count,
            LastAccessTime = _cacheStats.Values.Any() ? _cacheStats.Values.Max(s => s.LastAccessTime) : DateTime.MinValue,
            Uptime = TimeSpan.FromMinutes(5), // Placeholder - would need to track actual uptime
            TotalRequests = _cacheStats.Values.Sum(s => s.AccessCount),
            CacheHits = _cacheStats.Values.Sum(s => s.HitCount),
            CacheMisses = _cacheStats.Values.Sum(s => s.MissCount)
        };

        stats.HitRatio = stats.TotalRequests > 0 ? (double)stats.CacheHits / stats.TotalRequests : 0;

        return stats;
    }

    private void UpdateStats(string key, bool isHit)
    {
        var stats = _cacheStats.GetOrAdd(key, _ => new CacheEntryStats());
        stats.AccessCount++;
        stats.LastAccessTime = DateTime.UtcNow;

        if (isHit)
        {
            stats.HitCount++;
        }
        else
        {
            stats.MissCount++;
        }
    }
}

public class CacheEntryStats
{
    public int AccessCount { get; set; }
    public int HitCount { get; set; }
    public int MissCount { get; set; }
    public DateTime LastAccessTime { get; set; } = DateTime.UtcNow;
}
