using CommunityCar.Application.DTOs.Community;
using MediatR;

namespace CommunityCar.Application.Features.Badges.Queries;

public class GetReputationLeaderboardQuery : IRequest<IEnumerable<ReputationLeaderboardDto>>
{
    public int Limit { get; set; } = 50;
}