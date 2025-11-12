using CommunityCar.Application.DTOs.Community;
using CommunityCar.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommunityCar.Application.Features.Groups.Queries;

public class GetUserGroupsQueryHandler : IRequestHandler<GetUserGroupsQuery, IEnumerable<GroupDto>>
{
    private readonly IRepository<Domain.Entities.Community.Group> _groupRepository;

    public GetUserGroupsQueryHandler(IRepository<Domain.Entities.Community.Group> groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task<IEnumerable<GroupDto>> Handle(GetUserGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await _groupRepository.GetAll()
            .Where(g => !g.IsDeleted && g.IsActive)
            .Include(g => g.Members.Where(m => m.UserId == request.UserId && m.Status == Domain.Entities.Community.GroupMemberStatus.Active))
            .Include(g => g.Events.Where(e => !e.IsDeleted && e.StartDate > DateTime.UtcNow))
            .Where(g => g.Members.Any(m => m.UserId == request.UserId))
            .Select(g => new GroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                OwnerId = g.OwnerId,
                OwnerName = g.Owner.FirstName + " " + g.Owner.LastName,
                CoverImageUrl = g.CoverImageUrl,
                Privacy = g.Privacy,
                MemberCount = g.Members.Count(m => m.Status == Domain.Entities.Community.GroupMemberStatus.Active),
                EventCount = g.Events.Count(e => !e.IsDeleted),
                IsActive = g.IsActive,
                CreatedAt = g.CreatedAt,
                LastActivityAt = g.Events.Any() ? g.Events.Max(e => e.StartDate) : g.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return groups;
    }
}