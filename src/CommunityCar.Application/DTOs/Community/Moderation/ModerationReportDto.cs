using CommunityCar.Domain.Entities.Community;

namespace CommunityCar.Application.DTOs.Community.Moderation;

public class ModerationReportDto
{
    public int Id { get; set; }
    public string ReporterId { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public string ReportedUserId { get; set; } = string.Empty;
    public string ReportedUserName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int ContentId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? AdditionalInfo { get; set; }
    public ModerationReportStatus Status { get; set; }
    public string? ModeratorId { get; set; }
    public string? ModeratorName { get; set; }
    public string? Resolution { get; set; }
    public string? ModeratorNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}