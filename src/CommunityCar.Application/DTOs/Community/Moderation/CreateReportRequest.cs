namespace CommunityCar.Application.DTOs.Community.Moderation;

public class CreateReportRequest
{
    public string ReportedUserId { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int ContentId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? AdditionalInfo { get; set; }
}
