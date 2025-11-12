using CommunityCar.Domain.Entities.Community;

namespace CommunityCar.Application.DTOs.Community;

public class BadgeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public BadgeType Type { get; set; }
    public BadgeRarity Rarity { get; set; }
    public int PointsValue { get; set; }
    public string? Criteria { get; set; }
    public bool IsActive { get; set; }
    public int AwardedCount { get; set; }
}