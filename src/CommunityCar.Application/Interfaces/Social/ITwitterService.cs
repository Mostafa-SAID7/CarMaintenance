namespace CommunityCar.Application.Interfaces.Social;

public interface ITwitterService
{
    Task<TwitterUser> GetUserProfileAsync(string userId, string accessToken);
    Task<TwitterUser> GetUserByUsernameAsync(string username, string accessToken);
    Task<IEnumerable<TwitterTweet>> GetUserTweetsAsync(string userId, string accessToken, int maxResults = 10);
    Task<TwitterTweet> PostTweetAsync(string text, string accessToken, TwitterTweetMedia? media = null);
    Task<TwitterTweet> ReplyToTweetAsync(string tweetId, string text, string accessToken);
    Task<bool> LikeTweetAsync(string tweetId, string accessToken);
    Task<bool> RetweetAsync(string tweetId, string accessToken);
    Task<bool> UnlikeTweetAsync(string tweetId, string accessToken);
    Task<bool> UnretweetAsync(string tweetId, string accessToken);
    Task<TwitterTweet> GetTweetAsync(string tweetId, string accessToken);
    Task<IEnumerable<TwitterTweet>> SearchTweetsAsync(string query, string accessToken, int maxResults = 10);
    Task<IEnumerable<TwitterUser>> GetFollowersAsync(string userId, string accessToken, int maxResults = 100);
    Task<IEnumerable<TwitterUser>> GetFollowingAsync(string userId, string accessToken, int maxResults = 100);
    Task<bool> FollowUserAsync(string targetUserId, string accessToken);
    Task<bool> UnfollowUserAsync(string targetUserId, string accessToken);
    Task<TwitterTokenInfo> GetTokenInfoAsync(string accessToken);
    Task<string> ExchangeCodeForTokenAsync(string code, string codeVerifier, string redirectUri);
    Task<string> RefreshAccessTokenAsync(string refreshToken);
    Task<bool> ValidateAccessTokenAsync(string accessToken);
}

public class TwitterUser
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool Verified { get; set; }
    public bool Protected { get; set; }
    public DateTime CreatedAt { get; set; }
    public TwitterUserMetrics Metrics { get; set; } = new();
    public IEnumerable<string> PinnedTweetIds { get; set; } = new List<string>();
}

public class TwitterUserMetrics
{
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public int TweetCount { get; set; }
    public int ListedCount { get; set; }
}

public class TwitterTweet
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public TwitterUser? Author { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public string InReplyToUserId { get; set; } = string.Empty;
    public string ReferencedTweets { get; set; } = string.Empty;
    public IEnumerable<TwitterTweetAttachment> Attachments { get; set; } = new List<TwitterTweetAttachment>();
    public TwitterGeo? Geo { get; set; }
    public TwitterContextAnnotation? ContextAnnotations { get; set; }
    public TwitterEntities Entities { get; set; } = new();
    public TwitterTweetMetrics Metrics { get; set; } = new();
    public bool PossiblySensitive { get; set; }
    public string Lang { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string ReplySettings { get; set; } = string.Empty;
}

public class TwitterTweetMedia
{
    public IEnumerable<TwitterMediaItem> MediaItems { get; set; } = new List<TwitterMediaItem>();
}

public class TwitterMediaItem
{
    public string MediaId { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty; // photo, video, animated_gif
    public string Url { get; set; } = string.Empty;
    public int? Duration { get; set; } // for video
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string AltText { get; set; } = string.Empty;
}

public class TwitterTweetAttachment
{
    public IEnumerable<string> MediaKeys { get; set; } = new List<string>();
    public IEnumerable<string> PollIds { get; set; } = new List<string>();
}

public class TwitterGeo
{
    public string PlaceId { get; set; } = string.Empty;
    public TwitterCoordinates Coordinates { get; set; } = new();
}

public class TwitterCoordinates
{
    public string Type { get; set; } = string.Empty;
    public IEnumerable<double> CoordinatesList { get; set; } = new List<double>();
}

public class TwitterContextAnnotation
{
    public TwitterDomain Domain { get; set; } = new();
    public TwitterEntity Entity { get; set; } = new();
}

public class TwitterDomain
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class TwitterEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class TwitterEntities
{
    public IEnumerable<TwitterUrlEntity> Urls { get; set; } = new List<TwitterUrlEntity>();
    public IEnumerable<TwitterHashtagEntity> Hashtags { get; set; } = new List<TwitterHashtagEntity>();
    public IEnumerable<TwitterMentionEntity> Mentions { get; set; } = new List<TwitterMentionEntity>();
    public IEnumerable<TwitterCashtagEntity> Cashtags { get; set; } = new List<TwitterCashtagEntity>();
}

public class TwitterUrlEntity
{
    public int Start { get; set; }
    public int End { get; set; }
    public string Url { get; set; } = string.Empty;
    public string ExpandedUrl { get; set; } = string.Empty;
    public string DisplayUrl { get; set; } = string.Empty;
}

public class TwitterHashtagEntity
{
    public int Start { get; set; }
    public int End { get; set; }
    public string Tag { get; set; } = string.Empty;
}

public class TwitterMentionEntity
{
    public int Start { get; set; }
    public int End { get; set; }
    public string Username { get; set; } = string.Empty;
}

public class TwitterCashtagEntity
{
    public int Start { get; set; }
    public int End { get; set; }
    public string Tag { get; set; } = string.Empty;
}

public class TwitterTweetMetrics
{
    public int RetweetCount { get; set; }
    public int LikeCount { get; set; }
    public int ReplyCount { get; set; }
    public int QuoteCount { get; set; }
    public int ImpressionCount { get; set; }
}

public class TwitterTokenInfo
{
    public string TokenType { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsValid => DateTime.UtcNow < ExpiresAt;
}

public class TwitterApiException : Exception
{
    public string Title { get; }
    public string Detail { get; }
    public string Type { get; }
    public int Status { get; }

    public TwitterApiException(string message, string title, string detail, string type, int status)
        : base(message)
    {
        Title = title;
        Detail = detail;
        Type = type;
        Status = status;
    }
}

public class TwitterRateLimitExceededException : TwitterApiException
{
    public int ResetTime { get; }
    public int RemainingRequests { get; }

    public TwitterRateLimitExceededException(string message, int resetTime, int remainingRequests)
        : base(message, "Rate Limit Exceeded", "Too many requests", "about:blank", 429)
    {
        ResetTime = resetTime;
        RemainingRequests = remainingRequests;
    }
}
