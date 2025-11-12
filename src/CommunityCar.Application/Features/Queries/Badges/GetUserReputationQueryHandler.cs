using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Badges.Queries;

public class GetUserReputationQueryHandler : IRequestHandler<GetUserReputationQuery, ReputationScoreDto?>
{
    private readonly IBadgeService _badgeService;

    public GetUserReputationQueryHandler(IBadgeService badgeService)
    {
        _badgeService = badgeService;
    }

    public async Task<ReputationScoreDto?> Handle(GetUserReputationQuery request, CancellationToken cancellationToken)
    {
        return await _badgeService.GetUserReputationAsync(request.UserId);
    }
}
