using MediatR;

namespace CommunityCar.Application.Features.Moderation.Commands;

public class ResolveModerationReportCommand : IRequest<bool>
{
    public int ReportId { get; set; }
    public string ModeratorId { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
    public string? ModeratorNotes { get; set; }
}
