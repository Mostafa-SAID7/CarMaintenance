using MediatR;

namespace CommunityCar.Application.Features.Badges.Commands;

public class AwardBadgeCommand : IRequest<int>
{
    public int BadgeId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AwardedById { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}