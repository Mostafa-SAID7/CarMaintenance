namespace CommunityCar.Application.DTOs.Community.Moderation;

public class ModerateContentRequest
{
    public string Action { get; set; } = string.Empty; // "approve", "reject", "delete", "warn", "ban"
    public string Reason { get; set; } = string.Empty;
    public TimeSpan? Duration { get; set; } // For temporary bans
}