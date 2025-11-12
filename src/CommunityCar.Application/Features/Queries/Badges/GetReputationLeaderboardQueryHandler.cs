using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Badges.Queries;

public class GetReputationLeaderboardQueryHandler : IRequestHandler<GetReputationLeaderboardQuery, IEnumerable<ReputationLeaderboardDto>>
{
    private readonly IBadgeService _badgeService;

    public GetReputationLeaderboardQueryHandler(IBadgeService badgeService)
    {
        _badgeService = badgeService;
    }

    public async Task<IEnumerable<ReputationLeaderboardDto>> Handle(GetReputationLeaderboardQuery request, CancellationToken cancellationToken)
    {
        return await _badgeService.GetReputationLeaderboardAsync(request.Limit);
    }
}
