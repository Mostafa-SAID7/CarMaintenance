using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Badges.Queries;

public class GetBadgesQueryHandler : IRequestHandler<GetBadgesQuery, IEnumerable<BadgeDto>>
{
    private readonly IBadgeService _badgeService;

    public GetBadgesQueryHandler(IBadgeService badgeService)
    {
        _badgeService = badgeService;
    }

    public async Task<IEnumerable<BadgeDto>> Handle(GetBadgesQuery request, CancellationToken cancellationToken)
    {
        return await _badgeService.GetBadgesAsync();
    }
}