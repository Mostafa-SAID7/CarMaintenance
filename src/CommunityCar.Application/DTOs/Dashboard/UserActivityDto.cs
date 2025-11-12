namespace CommunityCar.Application.DTOs.Dashboard;

public class UserActivityDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime LastLoginAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public int LoginCount { get; set; }
    public int PostsCount { get; set; }
    public int CommentsCount { get; set; }
    public int VotesCount { get; set; }
    public int BadgesCount { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Inactive, Suspended
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}