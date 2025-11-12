namespace CommunityCar.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsersToday { get; set; }
    public int SuspendedUsers { get; set; }
    public int TotalPosts { get; set; }
    public int TotalComments { get; set; }
    public int TotalForums { get; set; }
    public int TotalGroups { get; set; }
    public int PendingReports { get; set; }
    public int TotalBadges { get; set; }
    public decimal AverageResponseTime { get; set; }
    public int TotalApiRequests { get; set; }
    public DateTime LastUpdated { get; set; }
}