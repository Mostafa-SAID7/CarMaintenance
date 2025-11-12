namespace CommunityCar.Application.Interfaces.Social;

public interface IFacebookService
{
    Task<FacebookUserProfile> GetUserProfileAsync(string accessToken);
    Task<FacebookPageInfo> GetPageInfoAsync(string pageId, string accessToken);
    Task<IEnumerable<FacebookPost>> GetUserPostsAsync(string userId, string accessToken, int limit = 25);
    Task<IEnumerable<FacebookPost>> GetPagePostsAsync(string pageId, string accessToken, int limit = 25);
    Task<FacebookPost> CreatePostAsync(string pageId, string message, string accessToken, FacebookPostAttachment? attachment = null);
    Task<bool> LikePostAsync(string postId, string accessToken);
    Task<bool> CommentOnPostAsync(string postId, string message, string accessToken);
    Task<IEnumerable<FacebookComment>> GetPostCommentsAsync(string postId, string accessToken, int limit = 25);
    Task<FacebookEvent> CreateEventAsync(string pageId, FacebookEventData eventData, string accessToken);
    Task<IEnumerable<FacebookEvent>> GetPageEventsAsync(string pageId, string accessToken);
    Task<bool> ValidateAccessTokenAsync(string accessToken);
    Task<FacebookTokenInfo> GetTokenInfoAsync(string accessToken);
    Task<string> ExchangeCodeForTokenAsync(string code, string redirectUri);
    Task<string> RefreshAccessTokenAsync(string refreshToken);
}

public class FacebookUserProfile
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Birthday { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public IEnumerable<string> Friends { get; set; } = new List<string>();
    public DateTime LastUpdated { get; set; }
}

public class FacebookPageInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public FacebookLocation Location { get; set; } = new();
    public int Likes { get; set; }
    public int Followers { get; set; }
    public string Picture { get; set; } = string.Empty;
    public string Cover { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
}

public class FacebookLocation
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class FacebookPost
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Story { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedTime { get; set; }
    public string Type { get; set; } = string.Empty; // status, photo, video, link, etc.
    public string StatusType { get; set; } = string.Empty;
    public FacebookPostAttachment? Attachment { get; set; }
    public FacebookPostStats Stats { get; set; } = new();
    public IEnumerable<FacebookComment> Comments { get; set; } = new List<FacebookComment>();
    public IEnumerable<string> Likes { get; set; } = new List<string>();
}

public class FacebookPostAttachment
{
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public FacebookMedia? Media { get; set; }
}

public class FacebookMedia
{
    public string Image { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}

public class FacebookPostStats
{
    public int Likes { get; set; }
    public int Comments { get; set; }
    public int Shares { get; set; }
    public int Views { get; set; }
}

public class FacebookComment
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string FromId { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }
    public int Likes { get; set; }
    public IEnumerable<FacebookComment> Replies { get; set; } = new List<FacebookComment>();
}

public class FacebookEvent
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public FacebookLocation Location { get; set; } = new();
    public string Cover { get; set; } = string.Empty;
    public FacebookEventStats Stats { get; set; } = new();
    public IEnumerable<string> Attendees { get; set; } = new List<string>();
}

public class FacebookEventData
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Place { get; set; } = string.Empty;
    public string TicketUri { get; set; } = string.Empty;
}

public class FacebookEventStats
{
    public int Attending { get; set; }
    public int Interested { get; set; }
    public int Declined { get; set; }
}

public class FacebookTokenInfo
{
    public string AppId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public long ExpiresAt { get; set; }
    public bool IsValid { get; set; }
    public long IssuedAt { get; set; }
    public IEnumerable<string> Scopes { get; set; } = new List<string>();
}

public class FacebookApiException : Exception
{
    public string ErrorType { get; }
    public string ErrorCode { get; }
    public string ErrorSubcode { get; }

    public FacebookApiException(string message, string errorType, string errorCode, string errorSubcode)
        : base(message)
    {
        ErrorType = errorType;
        ErrorCode = errorCode;
        ErrorSubcode = errorSubcode;
    }
}