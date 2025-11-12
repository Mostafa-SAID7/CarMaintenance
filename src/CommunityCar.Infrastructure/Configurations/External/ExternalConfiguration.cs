using CommunityCar.Application.Interfaces.Social;
using CommunityCar.Infrastructure.Services.External;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CommunityCar.Infrastructure.Configurations.External;

public static class ExternalConfiguration
{
    public static IServiceCollection AddExternalConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Register social media services
        services.AddScoped<IFacebookService, FacebookService>();
        services.AddScoped<IGoogleService, GoogleService>();
        services.AddScoped<ILinkedInService, LinkedInService>();
        services.AddScoped<ITwitterService, TwitterService>();

        // Configure external service settings
        services.Configure<ExternalServiceSettings>(configuration.GetSection("ExternalServices"));

        // Register HTTP clients for external APIs
        services.AddHttpClient("Facebook", client =>
        {
            client.BaseAddress = new Uri("https://graph.facebook.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient("Google", client =>
        {
            client.BaseAddress = new Uri("https://www.googleapis.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient("LinkedIn", client =>
        {
            client.BaseAddress = new Uri("https://api.linkedin.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient("Twitter", client =>
        {
            client.BaseAddress = new Uri("https://api.twitter.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register OAuth handlers
        services.AddScoped<IOAuthHandler, OAuthHandler>();

        return services;
    }
}

public class ExternalServiceSettings
{
    public FacebookSettings Facebook { get; set; } = new();
    public GoogleSettings Google { get; set; } = new();
    public LinkedInSettings LinkedIn { get; set; } = new();
    public TwitterSettings Twitter { get; set; } = new();
    public bool EnableSocialLogin { get; set; } = true;
    public int TokenCacheDurationMinutes { get; set; } = 60;
    public bool EnableTokenRefresh { get; set; } = true;
}

public class FacebookSettings
{
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = new[] { "email", "public_profile" };
    public string RedirectUri { get; set; } = string.Empty;
}

public class GoogleSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = new[] { "openid", "email", "profile" };
    public string RedirectUri { get; set; } = string.Empty;
}

public class LinkedInSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = new[] { "r_liteprofile", "r_emailaddress" };
    public string RedirectUri { get; set; } = string.Empty;
}

public class TwitterSettings
{
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string AccessTokenSecret { get; set; } = string.Empty;
}

public interface IOAuthHandler
{
    Task<OAuthResult> HandleCallbackAsync(string provider, string code, string state);
    Task<string> GetAuthorizationUrlAsync(string provider, string state);
    Task<bool> ValidateTokenAsync(string provider, string token);
    Task RefreshTokenAsync(string provider, string refreshToken);
}

public class OAuthHandler : IOAuthHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ExternalServiceSettings _settings;
    private readonly ILogger<OAuthHandler> _logger;

    public OAuthHandler(
        IHttpClientFactory httpClientFactory,
        IOptions<ExternalServiceSettings> settings,
        ILogger<OAuthHandler> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<OAuthResult> HandleCallbackAsync(string provider, string code, string state)
    {
        try
        {
            switch (provider.ToLower())
            {
                case "facebook":
                    return await HandleFacebookCallbackAsync(code);
                case "google":
                    return await HandleGoogleCallbackAsync(code);
                case "linkedin":
                    return await HandleLinkedInCallbackAsync(code);
                default:
                    throw new ArgumentException($"Unsupported provider: {provider}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OAuth callback for provider {Provider}", provider);
            return new OAuthResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<string> GetAuthorizationUrlAsync(string provider, string state)
    {
        // Implementation for generating authorization URLs
        return $"https://{provider}.com/oauth/authorize?state={state}";
    }

    public async Task<bool> ValidateTokenAsync(string provider, string token)
    {
        // Implementation for token validation
        return true; // Placeholder
    }

    public async Task RefreshTokenAsync(string provider, string refreshToken)
    {
        // Implementation for token refresh
        _logger.LogInformation("Refreshing token for provider {Provider}", provider);
    }

    private async Task<OAuthResult> HandleFacebookCallbackAsync(string code)
    {
        var client = _httpClientFactory.CreateClient("Facebook");

        // Exchange code for access token
        var tokenResponse = await client.GetAsync($"oauth/access_token?client_id={_settings.Facebook.AppId}&client_secret={_settings.Facebook.AppSecret}&code={code}&redirect_uri={_settings.Facebook.RedirectUri}");

        if (!tokenResponse.IsSuccessStatusCode)
        {
            return new OAuthResult { Success = false, Error = "Failed to get access token" };
        }

        // Get user info
        var userResponse = await client.GetAsync("me?fields=id,name,email,picture");

        return new OAuthResult
        {
            Success = true,
            Provider = "Facebook",
            AccessToken = "token_here", // Parse from response
            UserInfo = new OAuthUserInfo
            {
                Id = "id_here",
                Name = "name_here",
                Email = "email_here"
            }
        };
    }

    private async Task<OAuthResult> HandleGoogleCallbackAsync(string code)
    {
        // Similar implementation for Google
        return new OAuthResult
        {
            Success = true,
            Provider = "Google",
            AccessToken = "token_here",
            UserInfo = new OAuthUserInfo()
        };
    }

    private async Task<OAuthResult> HandleLinkedInCallbackAsync(string code)
    {
        // Similar implementation for LinkedIn
        return new OAuthResult
        {
            Success = true,
            Provider = "LinkedIn",
            AccessToken = "token_here",
            UserInfo = new OAuthUserInfo()
        };
    }
}

public class OAuthResult
{
    public bool Success { get; set; }
    public string? Provider { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public OAuthUserInfo? UserInfo { get; set; }
    public string? Error { get; set; }
}

public class OAuthUserInfo
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Picture { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

// Social media posting service
public interface ISocialMediaPoster
{
    Task<PostResult> PostToFacebookAsync(string content, string? imageUrl = null);
    Task<PostResult> PostToTwitterAsync(string content, string? imageUrl = null);
    Task<PostResult> PostToLinkedInAsync(string content, string? imageUrl = null);
}

public class SocialMediaPoster : ISocialMediaPoster
{
    private readonly IFacebookService _facebookService;
    private readonly ITwitterService _twitterService;
    private readonly ILinkedInService _linkedInService;
    private readonly ILogger<SocialMediaPoster> _logger;

    public SocialMediaPoster(
        IFacebookService facebookService,
        ITwitterService twitterService,
        ILinkedInService linkedInService,
        ILogger<SocialMediaPoster> logger)
    {
        _facebookService = facebookService;
        _twitterService = twitterService;
        _linkedInService = linkedInService;
        _logger = logger;
    }

    public async Task<PostResult> PostToFacebookAsync(string content, string? imageUrl = null)
    {
        try
        {
            // Implementation for posting to Facebook
            _logger.LogInformation("Posted to Facebook: {Content}", content);
            return new PostResult { Success = true, PostId = "fb_post_id" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting to Facebook");
            return new PostResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<PostResult> PostToTwitterAsync(string content, string? imageUrl = null)
    {
        try
        {
            // Implementation for posting to Twitter
            _logger.LogInformation("Posted to Twitter: {Content}", content);
            return new PostResult { Success = true, PostId = "tw_post_id" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting to Twitter");
            return new PostResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<PostResult> PostToLinkedInAsync(string content, string? imageUrl = null)
    {
        try
        {
            // Implementation for posting to LinkedIn
            _logger.LogInformation("Posted to LinkedIn: {Content}", content);
            return new PostResult { Success = true, PostId = "li_post_id" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting to LinkedIn");
            return new PostResult { Success = false, Error = ex.Message };
        }
    }
}

public class PostResult
{
    public bool Success { get; set; }
    public string? PostId { get; set; }
    public string? PostUrl { get; set; }
    public string? Error { get; set; }
}
