using CommunityCar.Application.DTOs.Community;
using MediatR;

namespace CommunityCar.Application.Features.Moderation.Queries;

public class GetModerationStatisticsQuery : IRequest<ModerationStatisticsDto>
{
}