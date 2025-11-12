using CommunityCar.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CommunityCar.Infrastructure.Services.External;

public class TwitterService : ITwitterService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TwitterService> _logger;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly string _accessToken;
    private readonly string _accessTokenSecret;

    public TwitterService(HttpClient httpClient, ILogger<TwitterService> logger,
                         string apiKey, string apiSecret, string accessToken, string accessTokenSecret)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = apiKey;
        _apiSecret = apiSecret;
        _accessToken = accessToken;
        _accessTokenSecret = accessTokenSecret;

        _httpClient.BaseAddress = new Uri("https://api.twitter.com/2/");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<TwitterUser> GetUserProfileAsync(string username = null, string userId = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(username))
                queryParams.Add($"usernames={username}");
            if (!string.IsNullOrEmpty(userId))
                queryParams.Add($"ids={userId}");

            if (!queryParams.Any())
                throw new ArgumentException("Either username or userId must be provided");

            var fields = "user.fields=id,name,username,description,profile_image_url,location,public_metrics,verified,created_at";
            var url = $"users/by?{string.Join("&", queryParams)}&{fields}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            await SignRequestAsync(request);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Twitter user profile: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TwitterUserResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Data?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Twitter user profile");
            return null;
        }
    }

    public async Task<List<TwitterTweet>> GetUserTweetsAsync(string userId, int maxResults = 10, bool excludeReplies = false, bool excludeRetweets = false)
    {
        try
        {
            var url = $"users/{userId}/tweets?" +
                     $"max_results={maxResults}&" +
                     "tweet.fields=id,text,created_at,public_metrics,author_id,context_annotations,entities";

            if (excludeReplies)
                url += "&exclude=replies";
            if (excludeRetweets)
                url += "&exclude=retweets";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            await SignRequestAsync(request);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Twitter tweets: {StatusCode}", response.StatusCode);
                return new List<TwitterTweet>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TwitterTweetResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Data ?? new List<TwitterTweet>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Twitter tweets");
            return new List<TwitterTweet>();
        }
    }

    public async Task<TwitterTweet> PostTweetAsync(string text, string? replyToTweetId = null, List<string>? mediaIds = null)
    {
        try
        {
            var tweetData = new
            {
                text = text,
                reply = replyToTweetId != null ? new { in_reply_to_tweet_id = replyToTweetId } : null,
                media = mediaIds != null ? new { media_ids = mediaIds } : null
            };

            var json = JsonSerializer.Serialize(tweetData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "tweets")
            {
                Content = content
            };
            await SignRequestAsync(request);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TwitterTweetResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var tweet = result?.Data?.FirstOrDefault();
                _logger.LogInformation("Successfully posted tweet: {TweetId}", tweet?.Id);
                return tweet;
            }
            else
            {
                _logger.LogWarning("Failed to post tweet: {StatusCode}", response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting tweet");
            return null;
        }
    }

    public async Task<List<TwitterTweet>> SearchTweetsAsync(string query, int maxResults = 10, string? startTime = null, string? endTime = null)
    {
        try
        {
            var url = $"tweets/search/recent?" +
                     $"query={Uri.EscapeDataString(query)}&" +
                     $"max_results={maxResults}&" +
                     "tweet.fields=id,text,created_at,public_metrics,author_id,context_annotations,entities";

            if (!string.IsNullOrEmpty(startTime))
                url += $"&start_time={startTime}";
            if (!string.IsNullOrEmpty(endTime))
                url += $"&end_time={endTime}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            await SignRequestAsync(request);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to search Twitter tweets: {StatusCode}", response.StatusCode);
                return new List<TwitterTweet>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TwitterTweetResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Data ?? new List<TwitterTweet>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Twitter tweets");
            return new List<TwitterTweet>();
        }
    }

    public async Task<TwitterTweet> GetTweetAsync(string tweetId)
    {
        try
        {
            var url = $"tweets/{tweetId}?" +
                     "tweet.fields=id,text,created_at,public_metrics,author_id,context_annotations,entities,attachments";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            await SignRequestAsync(request);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Twitter tweet {TweetId}: {StatusCode}", tweetId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TwitterTweetResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Data?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Twitter tweet {TweetId}", tweetId);
            return null;
        }
    }

    public async Task<bool> LikeTweetAsync(string tweetId)
    {
        try
        {
            var likeData = new { tweet_id = tweetId };
            var json = JsonSerializer.Serialize(likeData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "users/me/likes")
            {
                Content = content
            };
            await SignRequestAsync(request);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully liked tweet: {TweetId}", tweetId);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to like tweet {TweetId}: {StatusCode}", tweetId, response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking tweet {TweetId}", tweetId);
            return false;
        }
    }

    public async Task<bool> RetweetAsync(string tweetId)
    {
        try
        {
            var retweetData = new { tweet_id = tweetId };
            var json = JsonSerializer.Serialize(retweetData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "users/me/retweets")
            {
                Content = content
            };
            await SignRequestAsync(request);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully retweeted tweet: {TweetId}", tweetId);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to retweet tweet {TweetId}: {StatusCode}", tweetId, response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retweeting tweet {TweetId}", tweetId);
            return false;
        }
    }

    public async Task<List<TwitterUser>> GetFollowersAsync(string userId, int maxResults = 100)
    {
        try
        {
            var url = $"users/{userId}/followers?" +
                     $"max_results={maxResults}&" +
                     "user.fields=id,name,username,description,profile_image_url,public_metrics,verified";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            await SignRequestAsync(request);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Twitter followers: {StatusCode}", response.StatusCode);
                return new List<TwitterUser>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TwitterUserResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Data ?? new List<TwitterUser>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Twitter followers");
            return new List<TwitterUser>();
        }
    }

    public async Task<List<TwitterUser>> GetFollowingAsync(string userId, int maxResults = 100)
    {
        try
        {
            var url = $"users/{userId}/following?" +
                     $"max_results={maxResults}&" +
                     "user.fields=id,name,username,description,profile_image_url,public_metrics,verified";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            await SignRequestAsync(request);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Twitter following: {StatusCode}", response.StatusCode);
                return new List<TwitterUser>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TwitterUserResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Data ?? new List<TwitterUser>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Twitter following");
            return new List<TwitterUser>();
        }
    }

    private async Task SignRequestAsync(HttpRequestMessage request)
    {
        // Twitter API v2 uses OAuth 1.0a for authentication
        // This is a simplified implementation - in production, use a proper OAuth library
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var nonce = Guid.NewGuid().ToString("N");

        var parameters = new Dictionary<string, string>
        {
            ["oauth_consumer_key"] = _apiKey,
            ["oauth_nonce"] = nonce,
            ["oauth_signature_method"] = "HMAC-SHA1",
            ["oauth_timestamp"] = timestamp,
            ["oauth_token"] = _accessToken,
            ["oauth_version"] = "1.0"
        };

        // Add query parameters if any
        if (request.RequestUri != null && !string.IsNullOrEmpty(request.RequestUri.Query))
        {
            var queryParams = System.Web.HttpUtility.ParseQueryString(request.RequestUri.Query);
            foreach (var key in queryParams.AllKeys)
            {
                if (!string.IsNullOrEmpty(key))
                    parameters[key] = queryParams[key] ?? "";
            }
        }

        // Create signature base string
        var signatureBaseString = $"{request.Method.Method.ToUpper()}&{Uri.EscapeDataString(_httpClient.BaseAddress + request.RequestUri.AbsolutePath)}&{Uri.EscapeDataString(string.Join("&", parameters.OrderBy(p => p.Key).Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}")))}";

        // Create signing key
        var signingKey = $"{Uri.EscapeDataString(_apiSecret)}&{Uri.EscapeDataString(_accessTokenSecret)}";

        // Generate signature
        using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(signingKey));
        var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureBaseString)));

        // Add OAuth header
        var authHeader = $"OAuth oauth_consumer_key=\"{Uri.EscapeDataString(_apiKey)}\", oauth_nonce=\"{Uri.EscapeDataString(nonce)}\", oauth_signature=\"{Uri.EscapeDataString(signature)}\", oauth_signature_method=\"HMAC-SHA1\", oauth_timestamp=\"{timestamp}\", oauth_token=\"{Uri.EscapeDataString(_accessToken)}\", oauth_version=\"1.0\"";

        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader.Replace("OAuth ", ""));
    }
}

public interface ITwitterService
{
    Task<TwitterUser> GetUserProfileAsync(string username = null, string userId = null);
    Task<List<TwitterTweet>> GetUserTweetsAsync(string userId, int maxResults = 10, bool excludeReplies = false, bool excludeRetweets = false);
    Task<TwitterTweet> PostTweetAsync(string text, string? replyToTweetId = null, List<string>? mediaIds = null);
    Task<List<TwitterTweet>> SearchTweetsAsync(string query, int maxResults = 10, string? startTime = null, string? endTime = null);
    Task<TwitterTweet> GetTweetAsync(string tweetId);
    Task<bool> LikeTweetAsync(string tweetId);
    Task<bool> RetweetAsync(string tweetId);
    Task<List<TwitterUser>> GetFollowersAsync(string userId, int maxResults = 100);
    Task<List<TwitterUser>> GetFollowingAsync(string userId, int maxResults = 100);
}

public class TwitterUserResponse
{
    public List<TwitterUser> Data { get; set; } = new();
}

public class TwitterUser
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public TwitterPublicMetrics PublicMetrics { get; set; } = new();
    public bool Verified { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TwitterPublicMetrics
{
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public int TweetCount { get; set; }
    public int ListedCount { get; set; }
}

public class TwitterTweetResponse
{
    public List<TwitterTweet> Data { get; set; } = new();
}

public class TwitterTweet
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public TwitterPublicMetrics PublicMetrics { get; set; } = new();
    public List<TwitterContextAnnotation> ContextAnnotations { get; set; } = new();
    public TwitterEntities Entities { get; set; } = new();
    public TwitterAttachments Attachments { get; set; } = new();
}

public class TwitterContextAnnotation
{
    public TwitterContextAnnotationDomain Domain { get; set; } = new();
    public TwitterContextAnnotationEntity Entity { get; set; } = new();
}

public class TwitterContextAnnotationDomain
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class TwitterContextAnnotationEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class TwitterEntities
{
    public List<TwitterHashtag> Hashtags { get; set; } = new();
    public List<TwitterMention> Mentions { get; set; } = new();
    public List<TwitterUrl> Urls { get; set; } = new();
}

public class TwitterHashtag
{
    public string Tag { get; set; } = string.Empty;
}

public class TwitterMention
{
    public string Username { get; set; } = string.Empty;
}

public class TwitterUrl
{
    public string Url { get; set; } = string.Empty;
    public string ExpandedUrl { get; set; } = string.Empty;
    public string DisplayUrl { get; set; } = string.Empty;
}

public class TwitterAttachments
{
    public List<string> MediaKeys { get; set; } = new();
}

