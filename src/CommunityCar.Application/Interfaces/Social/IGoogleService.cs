namespace CommunityCar.Application.Interfaces.Social;

public interface IGoogleService
{
    Task<GoogleUserProfile> GetUserProfileAsync(string accessToken);
    Task<GoogleTokenInfo> GetTokenInfoAsync(string accessToken);
    Task<string> ExchangeCodeForTokenAsync(string code, string redirectUri);
    Task<string> RefreshAccessTokenAsync(string refreshToken);
    Task<bool> ValidateAccessTokenAsync(string accessToken);
    Task<GoogleCalendar> GetUserCalendarsAsync(string accessToken);
    Task<GoogleCalendarEvent> CreateCalendarEventAsync(string calendarId, GoogleCalendarEventData eventData, string accessToken);
    Task<IEnumerable<GoogleCalendarEvent>> GetCalendarEventsAsync(string calendarId, string accessToken, DateTime? startDate = null, DateTime? endDate = null);
    Task<GoogleDriveFile> UploadFileAsync(Stream fileStream, string fileName, string mimeType, string accessToken, string? folderId = null);
    Task<GoogleDriveFile> GetFileAsync(string fileId, string accessToken);
    Task<IEnumerable<GoogleDriveFile>> GetFilesAsync(string accessToken, string? folderId = null);
    Task<bool> DeleteFileAsync(string fileId, string accessToken);
    Task<GmailMessage> SendEmailAsync(GmailMessageData messageData, string accessToken);
    Task<IEnumerable<GmailMessage>> GetEmailsAsync(string accessToken, string? query = null, int maxResults = 10);
    Task<GmailMessage> GetEmailAsync(string messageId, string accessToken);
    Task<bool> MarkEmailAsReadAsync(string messageId, string accessToken);
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
    public string Hd { get; set; } = string.Empty; // Hosted domain
    public DateTime LastUpdated { get; set; }
}

public class GoogleTokenInfo
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string IdToken { get; set; } = string.Empty;
    public GoogleUserInfo UserInfo { get; set; } = new();
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsValid => DateTime.UtcNow < ExpiresAt;
}

public class GoogleUserInfo
{
    public string Sub { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string Locale { get; set; } = string.Empty;
}

public class GoogleCalendar
{
    public string Id { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Primary { get; set; } = string.Empty;
    public string AccessRole { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public string ForegroundColor { get; set; } = string.Empty;
    public bool Selected { get; set; }
    public bool Hidden { get; set; }
}

public class GoogleCalendarEvent
{
    public string Id { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GoogleEventDateTime Start { get; set; } = new();
    public GoogleEventDateTime End { get; set; } = new();
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public IEnumerable<GoogleEventAttendee> Attendees { get; set; } = new List<GoogleEventAttendee>();
    public GoogleEventReminder Reminders { get; set; } = new();
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}

public class GoogleCalendarEventData
{
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GoogleEventDateTime Start { get; set; } = new();
    public GoogleEventDateTime End { get; set; } = new();
    public string Location { get; set; } = string.Empty;
    public IEnumerable<GoogleEventAttendee> Attendees { get; set; } = new List<GoogleEventAttendee>();
    public GoogleEventReminder Reminders { get; set; } = new();
}

public class GoogleEventDateTime
{
    public DateTime DateTime { get; set; }
    public string TimeZone { get; set; } = string.Empty;
}

public class GoogleEventAttendee
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool Optional { get; set; }
    public string ResponseStatus { get; set; } = string.Empty; // accepted, declined, tentative, needsAction
}

public class GoogleEventReminder
{
    public bool UseDefault { get; set; }
    public IEnumerable<GoogleReminderOverride> Overrides { get; set; } = new List<GoogleReminderOverride>();
}

public class GoogleReminderOverride
{
    public string Method { get; set; } = string.Empty; // email, popup
    public int Minutes { get; set; }
}

public class GoogleDriveFile
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string WebViewLink { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string ThumbnailLink { get; set; } = string.Empty;
    public bool Shared { get; set; }
    public IEnumerable<string> Parents { get; set; } = new List<string>();
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public GoogleDriveUser OwnedBy { get; set; } = new();
}

public class GoogleDriveUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
}

public class GmailMessage
{
    public string Id { get; set; } = string.Empty;
    public string ThreadId { get; set; } = string.Empty;
    public IEnumerable<GmailMessageLabel> LabelIds { get; set; } = new List<GmailMessageLabel>();
    public string Snippet { get; set; } = string.Empty;
    public GmailMessagePayload Payload { get; set; } = new();
    public long InternalDate { get; set; }
    public DateTime ReceivedDate => DateTimeOffset.FromUnixTimeMilliseconds(InternalDate).DateTime;
    public int SizeEstimate { get; set; }
}

public class GmailMessageData
{
    public GmailMessageAddress To { get; set; } = new();
    public GmailMessageAddress From { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; }
    public IEnumerable<GmailAttachment> Attachments { get; set; } = new List<GmailAttachment>();
}

public class GmailMessageAddress
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class GmailMessageLabel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class GmailMessagePayload
{
    public string MimeType { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public IEnumerable<GmailMessageHeader> Headers { get; set; } = new List<GmailMessageHeader>();
    public IEnumerable<GmailMessagePart> Parts { get; set; } = new List<GmailMessagePart>();
}

public class GmailMessageHeader
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class GmailMessagePart
{
    public string MimeType { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public GmailMessageBody Body { get; set; } = new();
    public IEnumerable<GmailMessagePart> Parts { get; set; } = new List<GmailMessagePart>();
}

public class GmailMessageBody
{
    public string Data { get; set; } = string.Empty;
    public int Size { get; set; }
}

public class GmailAttachment
{
    public string Filename { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

public class GoogleApiException : Exception
{
    public string Error { get; }
    public string ErrorDescription { get; }
    public int ErrorCode { get; }

    public GoogleApiException(string message, string error, string errorDescription, int errorCode)
        : base(message)
    {
        Error = error;
        ErrorDescription = errorDescription;
        ErrorCode = errorCode;
    }
}
