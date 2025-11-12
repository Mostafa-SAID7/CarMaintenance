using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces.Commuinty;
using MediatR;

namespace CommunityCar.Application.Features.Posts.Queries;

public class GetPostsByForumQueryHandler : IRequestHandler<GetPostsByForumQuery, IEnumerable<PostDto>>
{
    private readonly IPostService _postService;

    public GetPostsByForumQueryHandler(IPostService postService)
    {
        _postService = postService;
    }

    public async Task<IEnumerable<PostDto>> Handle(GetPostsByForumQuery request, CancellationToken cancellationToken)
    {
        return await _postService.GetPostsByForumAsync(request.ForumId, request.Page, request.PageSize);
    }
}