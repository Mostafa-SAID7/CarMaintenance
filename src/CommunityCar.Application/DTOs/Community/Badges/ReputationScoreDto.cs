using CommunityCar.Domain.Entities.Community;

namespace CommunityCar.Application.DTOs.Community;

public class ReputationScoreDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public int PostsScore { get; set; }
    public int CommentsScore { get; set; }
    public int VotesReceivedScore { get; set; }
    public int ModerationScore { get; set; }
    public int BadgesScore { get; set; }
    public ReputationLevel Level { get; set; }
    public int NextLevelThreshold { get; set; }
    public int BadgeCount { get; set; }
    public DateTime LastUpdated { get; set; }
}