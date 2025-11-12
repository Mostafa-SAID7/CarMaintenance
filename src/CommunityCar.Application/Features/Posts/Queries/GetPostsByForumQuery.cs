using CommunityCar.Application.DTOs.Community;
using MediatR;

namespace CommunityCar.Application.Features.Posts.Queries;

public class GetPostsByForumQuery : IRequest<IEnumerable<PostDto>>
{
    public int ForumId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}