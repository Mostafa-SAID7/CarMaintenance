using CommunityCar.Application.DTOs.Community;
using MediatR;

namespace CommunityCar.Application.Features.Badges.Queries;

public class GetUserBadgesQuery : IRequest<IEnumerable<UserBadgeDto>>
{
    public string UserId { get; set; } = string.Empty;
}
