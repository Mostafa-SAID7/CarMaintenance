using CommunityCar.Application.DTOs.Community;
using MediatR;

namespace CommunityCar.Application.Features.Comments.Queries;

public class GetCommentByIdQuery : IRequest<CommentDto?>
{
    public int CommentId { get; set; }
}