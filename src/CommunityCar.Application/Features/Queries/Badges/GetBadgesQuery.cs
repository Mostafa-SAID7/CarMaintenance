using CommunityCar.Application.DTOs.Community;
using MediatR;

namespace CommunityCar.Application.Features.Badges.Queries;

public class GetBadgesQuery : IRequest<IEnumerable<BadgeDto>>
{
}
