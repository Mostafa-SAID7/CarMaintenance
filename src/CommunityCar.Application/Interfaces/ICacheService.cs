using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommunityCar.Application.Interfaces;

public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache
    /// </summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Sets a value in cache
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Removes a value from cache
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Checks if a key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Gets or sets a value with a factory function
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Sets multiple values in cache
    /// </summary>
    Task SetMultipleAsync<T>(Dictionary<string, T> keyValuePairs, TimeSpan? expiration = null);

    /// <summary>
    /// Gets multiple values from cache
    /// </summary>
    Task<Dictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys);

    /// <summary>
    /// Removes multiple values from cache
    /// </summary>
    Task RemoveMultipleAsync(IEnumerable<string> keys);

    /// <summary>
    /// Clears all cache entries
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// Gets cache statistics
    /// </summary>
    Task<CacheStatistics> GetStatisticsAsync();
}

public class CacheStatistics
{
    public long TotalEntries { get; set; }
    public long TotalSize { get; set; }
    public DateTime LastAccessTime { get; set; }
    public TimeSpan Uptime { get; set; }
    public double HitRatio { get; set; }
    public long TotalRequests { get; set; }
    public long CacheHits { get; set; }
    public long CacheMisses { get; set; }
}