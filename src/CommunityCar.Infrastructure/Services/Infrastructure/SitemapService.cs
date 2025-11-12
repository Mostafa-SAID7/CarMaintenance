using CommunityCar.Application.Interfaces;
using CommunityCar.Domain.Entities.Community;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace CommunityCar.Infrastructure.Services.Infrastructure;

public class SitemapService : ISitemapService
{
    private readonly ILogger<SitemapService> _logger;
    private readonly ICacheService _cacheService;

    public SitemapService(ILogger<SitemapService> logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<string> GenerateSitemapXmlAsync()
    {
        var cacheKey = "sitemap:xml";
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            var urls = await GenerateSitemapUrlsAsync();

            var sitemap = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("urlset",
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                    new XAttribute(XNamespace.Xmlns + "schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd"),
                    from url in urls
                    select new XElement("url",
                        new XElement("loc", url.Location),
                        new XElement("lastmod", url.LastModified.ToString("yyyy-MM-dd")),
                        new XElement("changefreq", url.ChangeFrequency),
                        new XElement("priority", url.Priority.ToString("F1"))
                    )
                )
            );

            return sitemap.ToString();
        }, TimeSpan.FromHours(24)); // Cache for 24 hours
    }

    public async Task<List<SitemapUrl>> GenerateSitemapUrlsAsync()
    {
        var urls = new List<SitemapUrl>();

        // Static pages
        urls.AddRange(new[]
        {
            new SitemapUrl { Location = "/", LastModified = DateTime.UtcNow, ChangeFrequency = "daily", Priority = 1.0 },
            new SitemapUrl { Location = "/about", LastModified = DateTime.UtcNow, ChangeFrequency = "monthly", Priority = 0.8 },
            new SitemapUrl { Location = "/contact", LastModified = DateTime.UtcNow, ChangeFrequency = "monthly", Priority = 0.7 },
            new SitemapUrl { Location = "/privacy", LastModified = DateTime.UtcNow, ChangeFrequency = "yearly", Priority = 0.3 },
            new SitemapUrl { Location = "/terms", LastModified = DateTime.UtcNow, ChangeFrequency = "yearly", Priority = 0.3 },
            new SitemapUrl { Location = "/community", LastModified = DateTime.UtcNow, ChangeFrequency = "hourly", Priority = 0.9 },
            new SitemapUrl { Location = "/forums", LastModified = DateTime.UtcNow, ChangeFrequency = "hourly", Priority = 0.8 }
        });

        // Dynamic content - in a real implementation, these would come from repositories
        // Forums
        urls.AddRange(await GetForumUrlsAsync());

        // Posts
        urls.AddRange(await GetPostUrlsAsync());

        // User profiles (public ones)
        urls.AddRange(await GetPublicProfileUrlsAsync());

        // Groups
        urls.AddRange(await GetGroupUrlsAsync());

        return urls.OrderByDescending(u => u.Priority).ThenBy(u => u.Location).ToList();
    }

    public async Task SubmitSitemapToSearchEnginesAsync(string sitemapUrl)
    {
        var searchEngines = new[]
        {
            "https://www.google.com/ping?sitemap=",
            "https://www.bing.com/ping?sitemap=",
            "https://www.baidu.com/sitemap/push?continue=1&sitemap="
        };

        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        foreach (var engine in searchEngines)
        {
            try
            {
                var pingUrl = engine + Uri.EscapeDataString(sitemapUrl);
                var response = await httpClient.GetAsync(pingUrl);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully submitted sitemap to {Engine}", engine);
                }
                else
                {
                    _logger.LogWarning("Failed to submit sitemap to {Engine}: {StatusCode}", engine, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting sitemap to {Engine}", engine);
            }
        }
    }

    public async Task<SitemapIndex> GenerateSitemapIndexAsync()
    {
        // For large sites, sitemaps can be split into multiple files
        var sitemaps = new List<SitemapIndexEntry>
        {
            new SitemapIndexEntry
            {
                Location = "/sitemap-main.xml",
                LastModified = DateTime.UtcNow
            }
        };

        return new SitemapIndex
        {
            Sitemaps = sitemaps,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<string> GenerateSitemapIndexXmlAsync()
    {
        var index = await GenerateSitemapIndexAsync();

        var sitemapIndex = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("sitemapindex",
                new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                new XAttribute(XNamespace.Xmlns + "schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/siteindex.xsd"),
                from sitemap in index.Sitemaps
                select new XElement("sitemap",
                    new XElement("loc", sitemap.Location),
                    new XElement("lastmod", sitemap.LastModified.ToString("yyyy-MM-dd"))
                )
            )
        );

        return sitemapIndex.ToString();
    }

    public async Task<SitemapStats> GetSitemapStatsAsync()
    {
        var urls = await GenerateSitemapUrlsAsync();

        return new SitemapStats
        {
            TotalUrls = urls.Count,
            StaticPages = urls.Count(u => u.ChangeFrequency == "yearly" || u.ChangeFrequency == "monthly"),
            DynamicPages = urls.Count(u => u.ChangeFrequency != "yearly" && u.ChangeFrequency != "monthly"),
            HighPriorityPages = urls.Count(u => u.Priority >= 0.8),
            MediumPriorityPages = urls.Count(u => u.Priority >= 0.5 && u.Priority < 0.8),
            LowPriorityPages = urls.Count(u => u.Priority < 0.5),
            LastGenerated = DateTime.UtcNow,
            EstimatedSize = urls.Count * 100 // Rough estimate in bytes
        };
    }

    private async Task<List<SitemapUrl>> GetForumUrlsAsync()
    {
        // Mock data - in real implementation, query forum repository
        return new List<SitemapUrl>
        {
            new SitemapUrl { Location = "/forums/general", LastModified = DateTime.UtcNow.AddDays(-1), ChangeFrequency = "daily", Priority = 0.8 },
            new SitemapUrl { Location = "/forums/tech", LastModified = DateTime.UtcNow.AddDays(-2), ChangeFrequency = "daily", Priority = 0.8 },
            new SitemapUrl { Location = "/forums/help", LastModified = DateTime.UtcNow.AddHours(-12), ChangeFrequency = "hourly", Priority = 0.7 }
        };
    }

    private async Task<List<SitemapUrl>> GetPostUrlsAsync()
    {
        // Mock data - in real implementation, query post repository for recent/active posts
        return new List<SitemapUrl>
        {
            new SitemapUrl { Location = "/posts/1", LastModified = DateTime.UtcNow.AddHours(-1), ChangeFrequency = "hourly", Priority = 0.6 },
            new SitemapUrl { Location = "/posts/2", LastModified = DateTime.UtcNow.AddHours(-2), ChangeFrequency = "hourly", Priority = 0.6 },
            new SitemapUrl { Location = "/posts/3", LastModified = DateTime.UtcNow.AddHours(-3), ChangeFrequency = "hourly", Priority = 0.6 }
        };
    }

    private async Task<List<SitemapUrl>> GetPublicProfileUrlsAsync()
    {
        // Mock data - in real implementation, query user repository for public profiles
        return new List<SitemapUrl>
        {
            new SitemapUrl { Location = "/users/johndoe", LastModified = DateTime.UtcNow.AddDays(-7), ChangeFrequency = "weekly", Priority = 0.4 },
            new SitemapUrl { Location = "/users/janesmith", LastModified = DateTime.UtcNow.AddDays(-5), ChangeFrequency = "weekly", Priority = 0.4 }
        };
    }

    private async Task<List<SitemapUrl>> GetGroupUrlsAsync()
    {
        // Mock data - in real implementation, query group repository
        return new List<SitemapUrl>
        {
            new SitemapUrl { Location = "/groups/developers", LastModified = DateTime.UtcNow.AddDays(-3), ChangeFrequency = "daily", Priority = 0.7 },
            new SitemapUrl { Location = "/groups/designers", LastModified = DateTime.UtcNow.AddDays(-1), ChangeFrequency = "daily", Priority = 0.7 }
        };
    }
}

public interface ISitemapService
{
    Task<string> GenerateSitemapXmlAsync();
    Task<List<SitemapUrl>> GenerateSitemapUrlsAsync();
    Task SubmitSitemapToSearchEnginesAsync(string sitemapUrl);
    Task<SitemapIndex> GenerateSitemapIndexAsync();
    Task<string> GenerateSitemapIndexXmlAsync();
    Task<SitemapStats> GetSitemapStatsAsync();
}

public class SitemapUrl
{
    public string Location { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public string ChangeFrequency { get; set; } = "monthly"; // always, hourly, daily, weekly, monthly, yearly, never
    public double Priority { get; set; } = 0.5; // 0.0 to 1.0
}

public class SitemapIndex
{
    public List<SitemapIndexEntry> Sitemaps { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class SitemapIndexEntry
{
    public string Location { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
}

public class SitemapStats
{
    public int TotalUrls { get; set; }
    public int StaticPages { get; set; }
    public int DynamicPages { get; set; }
    public int HighPriorityPages { get; set; }
    public int MediumPriorityPages { get; set; }
    public int LowPriorityPages { get; set; }
    public DateTime LastGenerated { get; set; }
    public long EstimatedSize { get; set; } // in bytes
}

