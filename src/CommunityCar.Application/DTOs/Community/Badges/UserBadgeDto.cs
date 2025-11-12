using CommunityCar.Domain.Entities.Community;

namespace CommunityCar.Application.DTOs.Community;

public class UserBadgeDto
{
    public int Id { get; set; }
    public int BadgeId { get; set; }
    public BadgeDto Badge { get; set; } = null!;
    public string AwardedById { get; set; } = string.Empty;
    public string AwardedByName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime AwardedAt { get; set; }
    public bool IsDisplayed { get; set; }
}