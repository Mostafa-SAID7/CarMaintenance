using CommunityCar.Domain.Entities.Community;

namespace CommunityCar.Application.DTOs.Community;

public class ReputationLeaderboardDto
{
    public int Rank { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public ReputationLevel Level { get; set; }
    public int BadgeCount { get; set; }
}
