using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Moderation.Commands;

public class ResolveModerationReportCommandHandler : IRequestHandler<ResolveModerationReportCommand, bool>
{
    private readonly IModerationService _moderationService;

    public ResolveModerationReportCommandHandler(IModerationService moderationService)
    {
        _moderationService = moderationService;
    }

    public async Task<bool> Handle(ResolveModerationReportCommand request, CancellationToken cancellationToken)
    {
        var resolveRequest = new ResolveModerationReportRequest
        {
            ReportId = request.ReportId,
            ModeratorId = request.ModeratorId,
            Resolution = request.Resolution,
            ModeratorNotes = request.ModeratorNotes
        };

        return await _moderationService.ResolveModerationReportAsync(resolveRequest);
    }
}
