using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Forums.Queries;

public class GetForumCategoriesQueryHandler : IRequestHandler<GetForumCategoriesQuery, IEnumerable<ForumCategoryDto>>
{
    private readonly IForumService _forumService;

    public GetForumCategoriesQueryHandler(IForumService forumService)
    {
        _forumService = forumService;
    }

    public async Task<IEnumerable<ForumCategoryDto>> Handle(GetForumCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _forumService.GetForumCategoriesAsync();
    }
}