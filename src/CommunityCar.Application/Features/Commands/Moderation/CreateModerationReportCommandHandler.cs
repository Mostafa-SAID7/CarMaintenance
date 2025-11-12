using CommunityCar.Domain.Entities.Community;
using CommunityCar.Domain.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Moderation.Commands;

public class CreateModerationReportCommandHandler : IRequestHandler<CreateModerationReportCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<ModerationReport> _moderationReportRepository;

    public CreateModerationReportCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _moderationReportRepository = _unitOfWork.Repository<ModerationReport>();
    }

    public async Task<int> Handle(CreateModerationReportCommand request, CancellationToken cancellationToken)
    {
        var report = new ModerationReport
        {
            ReporterId = request.ReporterId,
            ReportedUserId = request.ReportedUserId,
            ContentType = request.ContentType,
            ContentId = request.ContentId,
            Reason = request.Reason,
            AdditionalInfo = request.AdditionalInfo,
            Status = ModerationReportStatus.Pending
        };

        await _moderationReportRepository.AddAsync(report);
        await _unitOfWork.SaveChangesAsync();

        return report.Id;
    }
}
