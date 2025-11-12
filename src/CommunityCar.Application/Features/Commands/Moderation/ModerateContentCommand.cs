using MediatR;

namespace CommunityCar.Application.Features.Moderation.Commands;

public class ModerateContentCommand : IRequest<bool>
{
    public string ContentType { get; set; } = string.Empty;
    public int ContentId { get; set; }
    public string ModeratorId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "approve", "reject", "delete", "warn", "ban"
    public string Reason { get; set; } = string.Empty;
    public TimeSpan? Duration { get; set; } // For temporary bans
}