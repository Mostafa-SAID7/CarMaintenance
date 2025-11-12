using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Badges.Commands;

public class AwardBadgeCommandHandler : IRequestHandler<AwardBadgeCommand, int>
{
    private readonly IBadgeService _badgeService;

    public AwardBadgeCommandHandler(IBadgeService badgeService)
    {
        _badgeService = badgeService;
    }

    public async Task<int> Handle(AwardBadgeCommand request, CancellationToken cancellationToken)
    {
        var awardRequest = new AwardBadgeRequest
        {
            BadgeId = request.BadgeId,
            UserId = request.UserId,
            AwardedById = request.AwardedById,
            Reason = request.Reason
        };

        return await _badgeService.AwardBadgeAsync(awardRequest);
    }
}
