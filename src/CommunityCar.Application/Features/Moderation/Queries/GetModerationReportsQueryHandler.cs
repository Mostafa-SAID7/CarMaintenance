using CommunityCar.Application.DTOs.Community;
using CommunityCar.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommunityCar.Application.Features.Moderation.Queries;

public class GetModerationReportsQueryHandler : IRequestHandler<GetModerationReportsQuery, IEnumerable<ModerationReportDto>>
{
    private readonly IRepository<Domain.Entities.Community.ModerationReport> _moderationReportRepository;

    public GetModerationReportsQueryHandler(IRepository<Domain.Entities.Community.ModerationReport> moderationReportRepository)
    {
        _moderationReportRepository = moderationReportRepository;
    }

    public async Task<IEnumerable<ModerationReportDto>> Handle(GetModerationReportsQuery request, CancellationToken cancellationToken)
    {
        var query = _moderationReportRepository.GetAll()
            .Include(r => r.Reporter)
            .Include(r => r.ReportedUser)
            .Include(r => r.Moderator)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<Domain.Entities.Community.ModerationReportStatus>(request.Status, true, out var status))
            {
                query = query.Where(r => r.Status == status);
            }
        }

        var skip = (request.Page - 1) * request.PageSize;

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(r => new ModerationReportDto
            {
                Id = r.Id,
                ReporterId = r.ReporterId,
                ReporterName = r.Reporter.FirstName + " " + r.Reporter.LastName,
                ReportedUserId = r.ReportedUserId,
                ReportedUserName = r.ReportedUser.FirstName + " " + r.ReportedUser.LastName,
                ContentType = r.ContentType,
                ContentId = r.ContentId,
                Reason = r.Reason,
                AdditionalInfo = r.AdditionalInfo,
                Status = r.Status,
                ModeratorId = r.ModeratorId,
                ModeratorName = r.Moderator != null ? r.Moderator.FirstName + " " + r.Moderator.LastName : null,
                Resolution = r.Resolution,
                ModeratorNotes = r.ModeratorNotes,
                CreatedAt = r.CreatedAt,
                ResolvedAt = r.ResolvedAt
            })
            .ToListAsync(cancellationToken);

        return reports;
    }
}