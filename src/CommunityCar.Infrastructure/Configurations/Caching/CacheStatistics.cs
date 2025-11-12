using System.Collections.Concurrent;
using CommunityCar.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Configurations.Caching;

/// <summary>
/// Service for collecting and managing cache statistics
/// </summary>
public interface ICacheStatisticsService
{
    /// <summary>
    /// Gets current cache statistics
    /// </summary>
    Task<CacheStatistics> GetStatisticsAsync();

    /// <summary>
    /// Records a cache hit
    /// </summary>
    Task RecordCacheHitAsync(string key);

    /// <summary>
    /// Records a cache miss
    /// </summary>
    Task RecordCacheMissAsync(string key);

    /// <summary>
    /// Resets all statistics
    /// </summary>
    Task ResetStatisticsAsync();
}

/// <summary>
/// Implementation of cache statistics service
/// </summary>
public class CacheStatisticsService : ICacheStatisticsService
{
    private readonly ConcurrentDictionary<string, CacheEntryStats> _stats;
    private readonly ILogger<CacheStatisticsService> _logger;
    private readonly object _resetLock = new();

    public CacheStatisticsService(ILogger<CacheStatisticsService> logger)
    {
        _stats = new ConcurrentDictionary<string, CacheEntryStats>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CacheStatistics> GetStatisticsAsync()
    {
        try
        {
            var stats = _stats.Values;

            return new CacheStatistics
            {
                TotalEntries = stats.Count(),
                TotalRequests = stats.Sum(s => s.AccessCount),
                CacheHits = stats.Sum(s => s.HitCount),
                CacheMisses = stats.Sum(s => s.MissCount),
                HitRatio = stats.Any() ? (double)stats.Sum(s => s.HitCount) / stats.Sum(s => s.AccessCount) : 0,
                LastUpdated = DateTime.UtcNow,
                TotalSize = 0 // Not tracked in this implementation
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache statistics");
            return new CacheStatistics
            {
                TotalEntries = 0,
                TotalRequests = 0,
                CacheHits = 0,
                CacheMisses = 0,
                HitRatio = 0,
                LastUpdated = DateTime.UtcNow,
                TotalSize = 0
            };
        }
    }

    public async Task RecordCacheHitAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Attempted to record cache hit with null or empty key");
            return;
        }

        try
        {
            var stats = _stats.GetOrAdd(key, _ => new CacheEntryStats());
            stats.AccessCount++;
            stats.HitCount++;
            stats.LastAccessTime = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording cache hit for key: {Key}", key);
        }
    }

    public async Task RecordCacheMissAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Attempted to record cache miss with null or empty key");
            return;
        }

        try
        {
            var stats = _stats.GetOrAdd(key, _ => new CacheEntryStats());
            stats.AccessCount++;
            stats.MissCount++;
            stats.LastAccessTime = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording cache miss for key: {Key}", key);
        }
    }

    public async Task ResetStatisticsAsync()
    {
        try
        {
            lock (_resetLock)
            {
                _stats.Clear();
            }
            _logger.LogInformation("Cache statistics reset");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting cache statistics");
            throw;
        }
    }
}

/// <summary>
/// Statistics for cache performance
/// </summary>
public class CacheStatistics
{
    /// <summary>
    /// Total number of entries in cache
    /// </summary>
    public long TotalEntries { get; set; }

    /// <summary>
    /// Total size of cached data in bytes
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// Last time any cache entry was accessed
    /// </summary>
    public DateTime LastAccessTime { get; set; }

    /// <summary>
    /// How long the cache has been running
    /// </summary>
    public TimeSpan Uptime { get; set; }

    /// <summary>
    /// Cache hit ratio (0.0 to 1.0)
    /// </summary>
    public double HitRatio { get; set; }

    /// <summary>
    /// Total number of cache requests
    /// </summary>
    public long TotalRequests { get; set; }

    /// <summary>
    /// Total number of cache hits
    /// </summary>
    public long CacheHits { get; set; }

    /// <summary>
    /// Total number of cache misses
    /// </summary>
    public long CacheMisses { get; set; }

    /// <summary>
    /// When these statistics were last updated
    /// </summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Statistics for individual cache entries
/// </summary>
public class CacheEntryStats
{
    /// <summary>
    /// Total number of times this entry was accessed
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// Number of cache hits for this entry
    /// </summary>
    public int HitCount { get; set; }

    /// <summary>
    /// Number of cache misses for this entry
    /// </summary>
    public int MissCount { get; set; }

    /// <summary>
    /// Last time this entry was accessed
    /// </summary>
    public DateTime LastAccessTime { get; set; } = DateTime.UtcNow;
}
