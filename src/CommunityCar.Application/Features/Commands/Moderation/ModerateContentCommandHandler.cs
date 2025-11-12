using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Moderation.Commands;

public class ModerateContentCommandHandler : IRequestHandler<ModerateContentCommand, bool>
{
    private readonly IModerationService _moderationService;

    public ModerateContentCommandHandler(IModerationService moderationService)
    {
        _moderationService = moderationService;
    }

    public async Task<bool> Handle(ModerateContentCommand request, CancellationToken cancellationToken)
    {
        var moderateRequest = new ModerateContentRequest
        {
            ContentType = request.ContentType,
            ContentId = request.ContentId,
            ModeratorId = request.ModeratorId,
            Action = request.Action,
            Reason = request.Reason,
            Duration = request.Duration
        };

        return await _moderationService.ModerateContentAsync(moderateRequest);
    }
}
