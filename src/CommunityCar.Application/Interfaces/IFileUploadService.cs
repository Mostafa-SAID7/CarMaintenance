namespace CommunityCar.Application.Interfaces;

public interface IFileUploadService
{
    Task<FileUploadResult> UploadFileAsync(IFormFile file, string? folder = null, string? fileName = null);
    Task<IEnumerable<FileUploadResult>> UploadFilesAsync(IEnumerable<IFormFile> files, string? folder = null);
    Task<FileDownloadResult> DownloadFileAsync(string fileId);
    Task<bool> DeleteFileAsync(string fileId);
    Task<bool> ExistsAsync(string fileId);
    Task<FileInfo> GetFileInfoAsync(string fileId);
    Task<string> GetFileUrlAsync(string fileId, TimeSpan? expiry = null);
    Task<long> GetTotalStorageUsedAsync(string? userId = null);
    Task<IEnumerable<FileInfo>> GetUserFilesAsync(string userId, int page = 1, int pageSize = 20);
    Task<bool> MoveFileAsync(string fileId, string newFolder);
    Task<bool> CopyFileAsync(string fileId, string destinationFolder);
    Task<FileUploadResult> ReplaceFileAsync(string fileId, IFormFile newFile);
}

public class FileUploadResult
{
    public bool Success { get; set; }
    public string? FileId { get; set; }
    public string? FileName { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
    public string? Folder { get; set; }
    public string? Url { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class FileDownloadResult
{
    public bool Success { get; set; }
    public Stream? Stream { get; set; }
    public string? ContentType { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public string? ErrorMessage { get; set; }
}

public class FileInfo
{
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Folder { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public FileStatus Status { get; set; } = FileStatus.Active;
}

public enum FileStatus
{
    Active,
    Deleted,
    Archived,
    Processing,
    Failed
}

public class FileUploadOptions
{
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
    public IEnumerable<string> AllowedExtensions { get; set; } = new[]
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv", ".zip", ".rar", ".7z",
        ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm",
        ".mp3", ".wav", ".flac", ".aac"
    };
    public IEnumerable<string> AllowedContentTypes { get; set; } = new[]
    {
        "image/", "video/", "audio/", "application/pdf",
        "application/msword", "application/vnd.openxmlformats",
        "text/", "application/zip"
    };
    public bool GenerateThumbnails { get; set; } = true;
    public int ThumbnailWidth { get; set; } = 200;
    public int ThumbnailHeight { get; set; } = 200;
    public bool UseUniqueFileNames { get; set; } = true;
    public string? DefaultFolder { get; set; }
    public bool AllowOverwrite { get; set; } = false;
    public TimeSpan UrlExpiry { get; set; } = TimeSpan.FromHours(24);
}

public interface IImageProcessingService
{
    Task<ImageProcessingResult> ProcessImageAsync(Stream imageStream, ImageProcessingOptions options);
    Task<ImageProcessingResult> GenerateThumbnailAsync(Stream imageStream, int width, int height);
    Task<ImageProcessingResult> ResizeImageAsync(Stream imageStream, int width, int height);
    Task<ImageProcessingResult> CropImageAsync(Stream imageStream, int x, int y, int width, int height);
    Task<ImageProcessingResult> ConvertImageFormatAsync(Stream imageStream, string targetFormat);
}

public class ImageProcessingResult
{
    public bool Success { get; set; }
    public Stream? ProcessedStream { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ImageProcessingOptions
{
    public int? MaxWidth { get; set; }
    public int? MaxHeight { get; set; }
    public int Quality { get; set; } = 90;
    public string? Format { get; set; }
    public bool MaintainAspectRatio { get; set; } = true;
    public bool GenerateThumbnail { get; set; } = true;
    public int ThumbnailWidth { get; set; } = 200;
    public int ThumbnailHeight { get; set; } = 200;
}

public class FileStorageOptions
{
    public string Provider { get; set; } = "Local"; // Local, Azure, AWS, Google
    public string BasePath { get; set; } = "uploads/";
    public string BaseUrl { get; set; } = "/files/";
    public bool UseCdn { get; set; } = false;
    public string? CdnUrl { get; set; }
    public long MaxStoragePerUser { get; set; } = 100 * 1024 * 1024; // 100MB
    public TimeSpan FileRetentionPeriod { get; set; } = TimeSpan.FromDays(365);
    public bool EnableVirusScanning { get; set; } = true;
    public bool EnableCompression { get; set; } = true;
}

public static class FileUploadExtensions
{
    public static bool IsValidFile(this IFormFile file, FileUploadOptions options)
    {
        if (file.Length > options.MaxFileSize)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!options.AllowedExtensions.Contains(extension))
            return false;

        if (!options.AllowedContentTypes.Any(ct => file.ContentType.StartsWith(ct)))
            return false;

        return true;
    }

    public static string GetUniqueFileName(this IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        return $"{Guid.NewGuid()}{extension}";
    }

    public static string GetSafeFileName(this IFormFile file)
    {
        var fileName = Path.GetFileName(file.FileName);
        return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
    }

    public static string GetFileSizeString(this long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }
}
