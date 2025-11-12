using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Moderation.Queries;

public class GetModerationStatisticsQueryHandler : IRequestHandler<GetModerationStatisticsQuery, ModerationStatisticsDto>
{
    private readonly IModerationService _moderationService;

    public GetModerationStatisticsQueryHandler(IModerationService moderationService)
    {
        _moderationService = moderationService;
    }

    public async Task<ModerationStatisticsDto> Handle(GetModerationStatisticsQuery request, CancellationToken cancellationToken)
    {
        return await _moderationService.GetModerationStatisticsAsync();
    }
}
