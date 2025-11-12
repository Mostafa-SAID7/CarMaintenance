namespace CommunityCar.Application.DTOs.Community.Moderation;

public class ModerationStatisticsDto
{
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int ResolvedReports { get; set; }
    public int TodayReports { get; set; }
    public int ThisWeekReports { get; set; }
    public Dictionary<string, int> ReportsByType { get; set; } = new();
    public Dictionary<string, int> ReportsByStatus { get; set; } = new();
}