using CommunityCar.Application.DTOs.Community;
using MediatR;

namespace CommunityCar.Application.Features.Badges.Queries;

public class GetUserReputationQuery : IRequest<ReputationScoreDto?>
{
    public string UserId { get; set; } = string.Empty;
}
