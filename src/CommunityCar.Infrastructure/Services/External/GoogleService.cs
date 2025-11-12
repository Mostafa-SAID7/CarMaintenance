using CommunityCar.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CommunityCar.Infrastructure.Services;

public class GoogleService : IGoogleService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleService> _logger;
    private readonly string _accessToken;

    public GoogleService(HttpClient httpClient, ILogger<GoogleService> logger, string accessToken)
    {
        _httpClient = httpClient;
        _logger = logger;
        _accessToken = accessToken;

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<GoogleUserProfile> GetUserProfileAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Google user profile: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<GoogleUserProfile>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Google user profile");
            return null;
        }
    }

    public async Task<GoogleUserProfile> GetUserProfileV3Async()
    {
        try
        {
            var response = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Google user profile v3: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<GoogleUserProfile>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Google user profile v3");
            return null;
        }
    }

    public async Task<List<GoogleCalendarEvent>> GetCalendarEventsAsync(string calendarId = "primary", DateTime? startDate = null, DateTime? endDate = null, int maxResults = 250)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow.AddDays(30);

            var url = $"https://www.googleapis.com/calendar/v3/calendars/{Uri.EscapeDataString(calendarId)}/events?" +
                     $"timeMin={start.ToString("yyyy-MM-ddTHH:mm:ssZ")}&" +
                     $"timeMax={end.ToString("yyyy-MM-ddTHH:mm:ssZ")}&" +
                     $"maxResults={maxResults}&" +
                     "singleEvents=true&" +
                     "orderBy=startTime";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Google Calendar events: {StatusCode}", response.StatusCode);
                return new List<GoogleCalendarEvent>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GoogleCalendarResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Items ?? new List<GoogleCalendarEvent>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Google Calendar events");
            return new List<GoogleCalendarEvent>();
        }
    }

    public async Task<GoogleCalendarEvent> CreateCalendarEventAsync(string calendarId, GoogleCalendarEventCreateRequest eventData)
    {
        try
        {
            var url = $"https://www.googleapis.com/calendar/v3/calendars/{Uri.EscapeDataString(calendarId)}/events";

            var json = JsonSerializer.Serialize(eventData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var createdEvent = JsonSerializer.Deserialize<GoogleCalendarEvent>(responseContent);
                _logger.LogInformation("Successfully created Google Calendar event: {EventId}", createdEvent?.Id);
                return createdEvent;
            }
            else
            {
                _logger.LogWarning("Failed to create Google Calendar event: {StatusCode}", response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Google Calendar event");
            return null;
        }
    }

    public async Task<List<GoogleDriveFile>> GetDriveFilesAsync(string query = null, int maxResults = 100)
    {
        try
        {
            var url = "https://www.googleapis.com/drive/v3/files?" +
                     $"pageSize={maxResults}&" +
                     "fields=files(id,name,mimeType,webViewLink,thumbnailLink,createdTime,modifiedTime,size,parents)";

            if (!string.IsNullOrEmpty(query))
            {
                url += $"&q={Uri.EscapeDataString(query)}";
            }

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Google Drive files: {StatusCode}", response.StatusCode);
                return new List<GoogleDriveFile>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GoogleDriveResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Files ?? new List<GoogleDriveFile>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Google Drive files");
            return new List<GoogleDriveFile>();
        }
    }

    public async Task<GoogleDriveFile> UploadFileAsync(string fileName, Stream fileStream, string mimeType, string parentId = null)
    {
        try
        {
            var metadata = new
            {
                name = fileName,
                parents = parentId != null ? new[] { parentId } : null
            };

            var metadataJson = JsonSerializer.Serialize(metadata);
            var metadataContent = new StringContent(metadataJson, System.Text.Encoding.UTF8, "application/json");

            var content = new MultipartContent();
            content.Add(metadataContent, "metadata");
            content.Add(new StreamContent(fileStream), "file");

            var response = await _httpClient.PostAsync("https://www.googleapis.com/upload/drive/v3/files?uploadType=multipart", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var uploadedFile = JsonSerializer.Deserialize<GoogleDriveFile>(responseContent);
                _logger.LogInformation("Successfully uploaded file to Google Drive: {FileId}", uploadedFile?.Id);
                return uploadedFile;
            }
            else
            {
                _logger.LogWarning("Failed to upload file to Google Drive: {StatusCode}", response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Google Drive");
            return null;
        }
    }

    public async Task<List<GoogleGmailMessage>> GetGmailMessagesAsync(int maxResults = 50, string query = null)
    {
        try
        {
            var url = $"https://www.googleapis.com/gmail/v1/users/me/messages?maxResults={maxResults}";
            if (!string.IsNullOrEmpty(query))
            {
                url += $"&q={Uri.EscapeDataString(query)}";
            }

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Gmail messages: {StatusCode}", response.StatusCode);
                return new List<GoogleGmailMessage>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GoogleGmailResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Messages ?? new List<GoogleGmailMessage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Gmail messages");
            return new List<GoogleGmailMessage>();
        }
    }

    public async Task<GoogleGmailMessageDetail> GetGmailMessageAsync(string messageId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://www.googleapis.com/gmail/v1/users/me/messages/{messageId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Gmail message {MessageId}: {StatusCode}", messageId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var message = JsonSerializer.Deserialize<GoogleGmailMessageDetail>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Gmail message {MessageId}", messageId);
            return null;
        }
    }

    public async Task<List<GoogleYouTubeVideo>> SearchYouTubeVideosAsync(string query, int maxResults = 25)
    {
        try
        {
            var url = $"https://www.googleapis.com/youtube/v3/search?" +
                     $"part=snippet&" +
                     $"q={Uri.EscapeDataString(query)}&" +
                     $"maxResults={maxResults}&" +
                     "type=video";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to search YouTube videos: {StatusCode}", response.StatusCode);
                return new List<GoogleYouTubeVideo>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GoogleYouTubeResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Items ?? new List<GoogleYouTubeVideo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching YouTube videos");
            return new List<GoogleYouTubeVideo>();
        }
    }

    public async Task<bool> ValidateAccessTokenAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v1/tokeninfo");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                return result?.ContainsKey("issued_to") ?? false;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google access token");
            return false;
        }
    }
}

public interface IGoogleService
{
    Task<GoogleUserProfile> GetUserProfileAsync();
    Task<GoogleUserProfile> GetUserProfileV3Async();
    Task<List<GoogleCalendarEvent>> GetCalendarEventsAsync(string calendarId = "primary", DateTime? startDate = null, DateTime? endDate = null, int maxResults = 250);
    Task<GoogleCalendarEvent> CreateCalendarEventAsync(string calendarId, GoogleCalendarEventCreateRequest eventData);
    Task<List<GoogleDriveFile>> GetDriveFilesAsync(string query = null, int maxResults = 100);
    Task<GoogleDriveFile> UploadFileAsync(string fileName, Stream fileStream, string mimeType, string parentId = null);
    Task<List<GoogleGmailMessage>> GetGmailMessagesAsync(int maxResults = 50, string query = null);
    Task<GoogleGmailMessageDetail> GetGmailMessageAsync(string messageId);
    Task<List<GoogleYouTubeVideo>> SearchYouTubeVideosAsync(string query, int maxResults = 25);
    Task<bool> ValidateAccessTokenAsync();
}

public class GoogleUserProfile
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool VerifiedEmail { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public string Locale { get; set; } = string.Empty;
    public string Hd { get; set; } = string.Empty;
}

public class GoogleCalendarResponse
{
    public List<GoogleCalendarEvent> Items { get; set; } = new();
    public string NextPageToken { get; set; } = string.Empty;
}

public class GoogleCalendarEvent
{
    public string Id { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GoogleCalendarDateTime Start { get; set; } = new();
    public GoogleCalendarDateTime End { get; set; } = new();
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string HtmlLink { get; set; } = string.Empty;
    public List<GoogleCalendarAttendee> Attendees { get; set; } = new();
}

public class GoogleCalendarDateTime
{
    public string DateTime { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
}

public class GoogleCalendarAttendee
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool Organizer { get; set; }
    public bool Self { get; set; }
    public string ResponseStatus { get; set; } = string.Empty;
}

public class GoogleCalendarEventCreateRequest
{
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GoogleCalendarDateTime Start { get; set; } = new();
    public GoogleCalendarDateTime End { get; set; } = new();
    public string Location { get; set; } = string.Empty;
    public List<GoogleCalendarAttendee> Attendees { get; set; } = new();
}

public class GoogleDriveResponse
{
    public List<GoogleDriveFile> Files { get; set; } = new();
    public string NextPageToken { get; set; } = string.Empty;
}

public class GoogleDriveFile
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string WebViewLink { get; set; } = string.Empty;
    public string ThumbnailLink { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public long? Size { get; set; }
    public List<string> Parents { get; set; } = new();
}

public class GoogleGmailResponse
{
    public List<GoogleGmailMessage> Messages { get; set; } = new();
    public string NextPageToken { get; set; } = string.Empty;
}

public class GoogleGmailMessage
{
    public string Id { get; set; } = string.Empty;
    public string ThreadId { get; set; } = string.Empty;
}

public class GoogleGmailMessageDetail
{
    public string Id { get; set; } = string.Empty;
    public string ThreadId { get; set; } = string.Empty;
    public List<string> LabelIds { get; set; } = new();
    public GoogleGmailMessagePayload Payload { get; set; } = new();
    public long InternalDate { get; set; }
}

public class GoogleGmailMessagePayload
{
    public string PartId { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public List<GoogleGmailMessageHeader> Headers { get; set; } = new();
    public string Body { get; set; } = new();
    public List<GoogleGmailMessagePart> Parts { get; set; } = new();
}

public class GoogleGmailMessageHeader
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class GoogleGmailMessagePart
{
    public string PartId { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public List<GoogleGmailMessageHeader> Headers { get; set; } = new();
    public string Body { get; set; } = new();
}

public class GoogleYouTubeResponse
{
    public List<GoogleYouTubeVideo> Items { get; set; } = new();
    public string NextPageToken { get; set; } = string.Empty;
}

public class GoogleYouTubeVideo
{
    public string Id { get; set; } = string.Empty;
    public GoogleYouTubeVideoSnippet Snippet { get; set; } = new();
}

public class GoogleYouTubeVideoSnippet
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ChannelTitle { get; set; } = string.Empty;
    public string PublishedAt { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public GoogleYouTubeThumbnail Thumbnails { get; set; } = new();
}

public class GoogleYouTubeThumbnail
{
    public GoogleYouTubeThumbnailInfo Default { get; set; } = new();
    public GoogleYouTubeThumbnailInfo Medium { get; set; } = new();
    public GoogleYouTubeThumbnailInfo High { get; set; } = new();
}

public class GoogleYouTubeThumbnailInfo
{
    public string Url { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}