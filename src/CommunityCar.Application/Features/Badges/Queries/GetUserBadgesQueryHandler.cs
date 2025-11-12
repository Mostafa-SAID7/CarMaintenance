using CommunityCar.Application.DTOs.Community;
using CommunityCar.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommunityCar.Application.Features.Badges.Queries;

public class GetUserBadgesQueryHandler : IRequestHandler<GetUserBadgesQuery, IEnumerable<UserBadgeDto>>
{
    private readonly IRepository<Domain.Entities.Community.UserBadge> _userBadgeRepository;

    public GetUserBadgesQueryHandler(IRepository<Domain.Entities.Community.UserBadge> userBadgeRepository)
    {
        _userBadgeRepository = userBadgeRepository;
    }

    public async Task<IEnumerable<UserBadgeDto>> Handle(GetUserBadgesQuery request, CancellationToken cancellationToken)
    {
        var userBadges = await _userBadgeRepository.GetAll()
            .Where(ub => ub.UserId == request.UserId && !ub.IsDeleted)
            .Include(ub => ub.Badge)
            .Include(ub => ub.AwardedBy)
            .OrderByDescending(ub => ub.AwardedAt)
            .Select(ub => new UserBadgeDto
            {
                Id = ub.Id,
                BadgeId = ub.BadgeId,
                Badge = new BadgeDto
                {
                    Id = ub.Badge.Id,
                    Name = ub.Badge.Name,
                    Description = ub.Badge.Description,
                    IconUrl = ub.Badge.IconUrl,
                    Type = ub.Badge.Type,
                    Rarity = ub.Badge.Rarity,
                    PointsValue = ub.Badge.PointsValue,
                    Criteria = ub.Badge.Criteria,
                    IsActive = ub.Badge.IsActive,
                    AwardedCount = ub.Badge.UserBadges.Count
                },
                AwardedById = ub.AwardedById,
                AwardedByName = ub.AwardedBy.FirstName + " " + ub.AwardedBy.LastName,
                Reason = ub.Reason,
                AwardedAt = ub.AwardedAt,
                IsDisplayed = ub.IsDisplayed
            })
            .ToListAsync(cancellationToken);

        return userBadges;
    }
}