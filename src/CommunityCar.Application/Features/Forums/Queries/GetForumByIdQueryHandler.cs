using CommunityCar.Application.DTOs.Community;
using CommunityCar.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommunityCar.Application.Features.Forums.Queries;

public class GetForumByIdQueryHandler : IRequestHandler<GetForumByIdQuery, ForumDto?>
{
    private readonly IRepository<Domain.Entities.Community.Forum> _forumRepository;

    public GetForumByIdQueryHandler(IRepository<Domain.Entities.Community.Forum> forumRepository)
    {
        _forumRepository = forumRepository;
    }

    public async Task<ForumDto?> Handle(GetForumByIdQuery request, CancellationToken cancellationToken)
    {
        var forum = await _forumRepository.GetAll()
            .Where(f => f.Id == request.ForumId && !f.IsDeleted && f.IsActive)
            .Include(f => f.Categories.Where(c => !c.IsDeleted && c.IsActive))
            .Include(f => f.Posts.Where(p => !p.IsDeleted && p.IsApproved))
            .FirstOrDefaultAsync(cancellationToken);

        if (forum == null)
            return null;

        return new ForumDto
        {
            Id = forum.Id,
            Name = forum.Name,
            Description = forum.Description,
            IconUrl = forum.IconUrl,
            DisplayOrder = forum.DisplayOrder,
            IsActive = forum.IsActive,
            PostCount = forum.Posts.Count,
            LastActivityAt = forum.Posts.Any() ? forum.Posts.Max(p => p.LastActivityAt ?? p.CreatedAt) : null
        };
    }
}