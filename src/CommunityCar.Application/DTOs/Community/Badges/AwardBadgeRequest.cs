namespace CommunityCar.Application.DTOs.Community;

public class AwardBadgeRequest
{
    public int BadgeId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}