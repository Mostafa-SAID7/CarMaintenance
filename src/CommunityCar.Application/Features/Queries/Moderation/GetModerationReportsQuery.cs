using CommunityCar.Application.DTOs.Community;
using MediatR;

namespace CommunityCar.Application.Features.Moderation.Queries;

public class GetModerationReportsQuery : IRequest<IEnumerable<ModerationReportDto>>
{
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
