using CommunityCar.Application.Interfaces.Community;
using CommunityCar.Domain.Entities.Community;
using CommunityCar.Infrastructure.Services.Content;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CommunityCar.Infrastructure.Configurations.Content;

public static class ContentConfiguration
{
    public static IServiceCollection AddContentConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Register content services
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IPostService, PostService>();

        // Configure content settings
        services.Configure<ContentSettings>(configuration.GetSection("Content"));

        // Register repositories for content
        services.AddScoped<IRepository<Post>, BaseRepository<Post>>();
        services.AddScoped<IRepository<Comment>, BaseRepository<Comment>>();
        services.AddScoped<IRepository<PostVote>, BaseRepository<PostVote>>();
        services.AddScoped<IRepository<CommentVote>, BaseRepository<CommentVote>>();
        services.AddScoped<IRepository<Tag>, BaseRepository<Tag>>();
        services.AddScoped<IRepository<PostTag>, BaseRepository<PostTag>>();

        // Register content moderation services
        services.AddScoped<IContentModerator, ContentModerator>();
        services.AddScoped<IContentFilter, ContentFilter>();

        // Register file upload service
        services.AddScoped<IFileUploadService, FileUploadService>();

        return services;
    }
}

public class ContentSettings
{
    public int MaxPostLength { get; set; } = 10000;
    public int MaxCommentLength { get; set; } = 2000;
    public int MaxTitleLength { get; set; } = 200;
    public bool EnableContentModeration { get; set; } = true;
    public bool EnableAutoModeration { get; set; } = true;
    public int MaxTagsPerPost { get; set; } = 5;
    public int MaxImagesPerPost { get; set; } = 10;
    public long MaxImageSizeBytes { get; set; } = 5242880; // 5MB
    public string[] AllowedImageTypes { get; set; } = new[] { "jpg", "jpeg", "png", "gif" };
    public bool EnableVoting { get; set; } = true;
    public int VoteCooldownMinutes { get; set; } = 5;
    public bool EnableRichText { get; set; } = true;
    public bool EnableMentions { get; set; } = true;
    public bool EnableHashtags { get; set; } = true;
}

public interface IContentModerator
{
    Task<ModerationResult> ModerateContentAsync(string content, string contentType);
    Task<bool> IsContentAllowedAsync(string content);
    Task ReportContentAsync(string contentId, string reason, string reportedBy);
}

public class ContentModerator : IContentModerator
{
    private readonly ContentSettings _settings;
    private readonly ILogger<ContentModerator> _logger;

    public ContentModerator(IOptions<ContentSettings> settings, ILogger<ContentModerator> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<ModerationResult> ModerateContentAsync(string content, string contentType)
    {
        var result = new ModerationResult
        {
            IsApproved = true,
            Confidence = 1.0,
            Flags = new List<string>()
        };

        // Basic content checks
        if (ContainsProfanity(content))
        {
            result.IsApproved = false;
            result.Flags.Add("profanity");
            result.Confidence = 0.9;
        }

        if (ContainsSpam(content))
        {
            result.IsApproved = false;
            result.Flags.Add("spam");
            result.Confidence = 0.8;
        }

        if (IsTooLong(content, contentType))
        {
            result.IsApproved = false;
            result.Flags.Add("too_long");
            result.Confidence = 1.0;
        }

        _logger.LogInformation("Content moderation result: {Result} for {ContentType}", result.IsApproved, contentType);

        return result;
    }

    public async Task<bool> IsContentAllowedAsync(string content)
    {
        var result = await ModerateContentAsync(content, "general");
        return result.IsApproved;
    }

    public async Task ReportContentAsync(string contentId, string reason, string reportedBy)
    {
        // Implementation for reporting content
        _logger.LogInformation("Content {ContentId} reported by {ReportedBy} for reason: {Reason}",
            contentId, reportedBy, reason);
    }

    private bool ContainsProfanity(string content)
    {
        // Simple profanity check - in real implementation, use a proper profanity filter
        var profanityWords = new[] { "badword1", "badword2" }; // Configure from settings
        return profanityWords.Any(word => content.Contains(word, StringComparison.OrdinalIgnoreCase));
    }

    private bool ContainsSpam(string content)
    {
        // Simple spam detection - in real implementation, use ML models
        var spamIndicators = new[] { "buy now", "click here", "free money" };
        return spamIndicators.Any(indicator => content.Contains(indicator, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsTooLong(string content, string contentType)
    {
        return contentType switch
        {
            "post" => content.Length > _settings.MaxPostLength,
            "comment" => content.Length > _settings.MaxCommentLength,
            "title" => content.Length > _settings.MaxTitleLength,
            _ => false
        };
    }
}

public class ModerationResult
{
    public bool IsApproved { get; set; }
    public double Confidence { get; set; }
    public List<string> Flags { get; set; } = new();
    public string? ModeratorNote { get; set; }
}

public interface IContentFilter
{
    Task<string> FilterContentAsync(string content);
    Task<bool> ContainsBlockedContentAsync(string content);
}

public class ContentFilter : IContentFilter
{
    private readonly ContentSettings _settings;

    public ContentFilter(IOptions<ContentSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<string> FilterContentAsync(string content)
    {
        // Basic content filtering - replace sensitive words
        var filtered = content;
        // Implementation would filter profanity, etc.
        return filtered;
    }

    public async Task<bool> ContainsBlockedContentAsync(string content)
    {
        // Check for blocked content
        return false; // Placeholder
    }
}

public interface IFileUploadService
{
    Task<FileUploadResult> UploadFileAsync(IFormFile file, string folder);
    Task<bool> DeleteFileAsync(string filePath);
    Task<long> GetFileSizeAsync(string filePath);
    Task<bool> FileExistsAsync(string filePath);
}

public class FileUploadService : IFileUploadService
{
    private readonly ContentSettings _settings;
    private readonly ILogger<FileUploadService> _logger;

    public FileUploadService(IOptions<ContentSettings> settings, ILogger<FileUploadService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<FileUploadResult> UploadFileAsync(IFormFile file, string folder)
    {
        // Validate file
        if (file.Length > _settings.MaxImageSizeBytes)
        {
            return new FileUploadResult { Success = false, Error = "File too large" };
        }

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!_settings.AllowedImageTypes.Contains(extension.TrimStart('.')))
        {
            return new FileUploadResult { Success = false, Error = "File type not allowed" };
        }

        // Generate unique filename
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folder, fileName);

        // In a real implementation, save to storage (local, cloud, etc.)
        // For now, just return success
        _logger.LogInformation("File uploaded: {FileName} to {Folder}", fileName, folder);

        return new FileUploadResult
        {
            Success = true,
            FileName = fileName,
            FilePath = filePath,
            FileSize = file.Length,
            ContentType = file.ContentType
        };
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        // Implementation for deleting files
        _logger.LogInformation("File deleted: {FilePath}", filePath);
        return true;
    }

    public async Task<long> GetFileSizeAsync(string filePath)
    {
        // Implementation for getting file size
        return 0;
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        // Implementation for checking file existence
        return true;
    }
}

public class FileUploadResult
{
    public bool Success { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public string? ContentType { get; set; }
    public string? Error { get; set; }
}
