using CommunityCar.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CommunityCar.Infrastructure.Services.External;

public class FacebookService : IFacebookService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FacebookService> _logger;
    private readonly string _accessToken;
    private readonly string _appSecret;

    public FacebookService(HttpClient httpClient, ILogger<FacebookService> logger, string accessToken, string appSecret = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        _accessToken = accessToken;
        _appSecret = appSecret;

        _httpClient.BaseAddress = new Uri("https://graph.facebook.com/v18.0/");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<FacebookUser> GetUserProfileAsync(string userId = "me")
    {
        try
        {
            var fields = "id,name,email,first_name,last_name,picture,link,about,birthday,gender,location,hometown,website,work,education";
            var response = await _httpClient.GetAsync($"{userId}?fields={fields}&access_token={_accessToken}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Facebook user profile: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<FacebookUser>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Facebook user profile");
            return null;
        }
    }

    public async Task<List<FacebookPost>> GetUserPostsAsync(string userId = "me", int limit = 25)
    {
        try
        {
            var fields = "id,message,story,created_time,updated_time,type,link,name,caption,description,picture,full_picture,source,likes.summary(true),comments.summary(true),shares";
            var response = await _httpClient.GetAsync($"{userId}/posts?fields={fields}&limit={limit}&access_token={_accessToken}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Facebook posts: {StatusCode}", response.StatusCode);
                return new List<FacebookPost>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FacebookResponse<FacebookPost>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Data ?? new List<FacebookPost>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Facebook posts");
            return new List<FacebookPost>();
        }
    }

    public async Task<bool> PostToFeedAsync(string message, string? link = null, string? picture = null)
    {
        try
        {
            var postData = new Dictionary<string, string>
            {
                ["message"] = message,
                ["access_token"] = _accessToken
            };

            if (!string.IsNullOrEmpty(link))
                postData["link"] = link;

            if (!string.IsNullOrEmpty(picture))
                postData["picture"] = picture;

            var content = new FormUrlEncodedContent(postData);
            var response = await _httpClient.PostAsync("me/feed", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FacebookPostResult>(responseContent);
                _logger.LogInformation("Successfully posted to Facebook feed: {PostId}", result?.Id);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to post to Facebook feed: {StatusCode}", response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting to Facebook feed");
            return false;
        }
    }

    public async Task<List<FacebookPage>> GetUserPagesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"me/accounts?access_token={_accessToken}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Facebook pages: {StatusCode}", response.StatusCode);
                return new List<FacebookPage>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FacebookResponse<FacebookPage>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Data ?? new List<FacebookPage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Facebook pages");
            return new List<FacebookPage>();
        }
    }

    public async Task<bool> PostToPageAsync(string pageId, string pageAccessToken, string message, string? link = null)
    {
        try
        {
            var postData = new Dictionary<string, string>
            {
                ["message"] = message,
                ["access_token"] = pageAccessToken
            };

            if (!string.IsNullOrEmpty(link))
                postData["link"] = link;

            var content = new FormUrlEncodedContent(postData);
            var response = await _httpClient.PostAsync($"{pageId}/feed", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully posted to Facebook page: {PageId}", pageId);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to post to Facebook page {PageId}: {StatusCode}", pageId, response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting to Facebook page {PageId}", pageId);
            return false;
        }
    }

    public async Task<FacebookEvent> CreateEventAsync(string pageId, string pageAccessToken, FacebookEventCreateRequest eventData)
    {
        try
        {
            var postData = new Dictionary<string, string>
            {
                ["name"] = eventData.Name,
                ["description"] = eventData.Description,
                ["start_time"] = eventData.StartTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ["end_time"] = eventData.EndTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ["access_token"] = pageAccessToken
            };

            if (!string.IsNullOrEmpty(eventData.Location))
                postData["location"] = eventData.Location;

            var content = new FormUrlEncodedContent(postData);
            var response = await _httpClient.PostAsync($"{pageId}/events", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FacebookEvent>(responseContent);
                _logger.LogInformation("Successfully created Facebook event: {EventId}", result?.Id);
                return result;
            }
            else
            {
                _logger.LogWarning("Failed to create Facebook event: {StatusCode}", response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Facebook event");
            return null;
        }
    }

    public async Task<bool> ValidateAccessTokenAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"me?fields=id&access_token={_accessToken}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                return result?.ContainsKey("id") ?? false;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Facebook access token");
            return false;
        }
    }
}

public interface IFacebookService
{
    Task<FacebookUser> GetUserProfileAsync(string userId = "me");
    Task<List<FacebookPost>> GetUserPostsAsync(string userId = "me", int limit = 25);
    Task<bool> PostToFeedAsync(string message, string? link = null, string? picture = null);
    Task<List<FacebookPage>> GetUserPagesAsync();
    Task<bool> PostToPageAsync(string pageId, string pageAccessToken, string message, string? link = null);
    Task<FacebookEvent> CreateEventAsync(string pageId, string pageAccessToken, FacebookEventCreateRequest eventData);
    Task<bool> ValidateAccessTokenAsync();
}

public class FacebookResponse<T>
{
    public List<T> Data { get; set; } = new();
    public FacebookPaging Paging { get; set; } = new();
}

public class FacebookPaging
{
    public string Next { get; set; } = string.Empty;
    public string Previous { get; set; } = string.Empty;
}

public class FacebookUser
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public FacebookPicture Picture { get; set; } = new();
    public string Link { get; set; } = string.Empty;
    public string About { get; set; } = string.Empty;
    public string Birthday { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public FacebookLocation Location { get; set; } = new();
    public FacebookLocation Hometown { get; set; } = new();
    public string Website { get; set; } = string.Empty;
    public List<FacebookWork> Work { get; set; } = new();
    public List<FacebookEducation> Education { get; set; } = new();
}

public class FacebookPicture
{
    public FacebookPictureData Data { get; set; } = new();
}

public class FacebookPictureData
{
    public string Url { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}

public class FacebookLocation
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class FacebookWork
{
    public string Employer { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public FacebookDateRange DateRange { get; set; } = new();
}

public class FacebookEducation
{
    public string School { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public FacebookDateRange DateRange { get; set; } = new();
}

public class FacebookDateRange
{
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
}

public class FacebookPost
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Story { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedTime { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public string FullPicture { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public FacebookLikes Likes { get; set; } = new();
    public FacebookComments Comments { get; set; } = new();
    public FacebookShares Shares { get; set; } = new();
}

public class FacebookLikes
{
    public int Count { get; set; }
}

public class FacebookComments
{
    public int Count { get; set; }
}

public class FacebookShares
{
    public int Count { get; set; }
}

public class FacebookPostResult
{
    public string Id { get; set; } = string.Empty;
}

public class FacebookPage
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Perms { get; set; } = new();
}

public class FacebookEvent
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class FacebookEventCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
}

