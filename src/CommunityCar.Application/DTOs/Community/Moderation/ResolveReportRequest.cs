namespace CommunityCar.Application.DTOs.Community.Moderation;

public class ResolveReportRequest
{
    public string Resolution { get; set; } = string.Empty;
    public string? ModeratorNotes { get; set; }
}
