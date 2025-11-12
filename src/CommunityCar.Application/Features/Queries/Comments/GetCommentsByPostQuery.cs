using CommunityCar.Application.DTOs.Community;
using MediatR;

namespace CommunityCar.Application.Features.Comments.Queries;

public class GetCommentsByPostQuery : IRequest<IEnumerable<CommentDto>>
{
    public int PostId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
