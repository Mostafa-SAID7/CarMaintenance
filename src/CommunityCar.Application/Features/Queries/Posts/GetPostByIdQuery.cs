using CommunityCar.Application.DTOs.Community;
using MediatR;

namespace CommunityCar.Application.Features.Posts.Queries;

public class GetPostByIdQuery : IRequest<PostDto?>
{
    public int PostId { get; set; }
}
