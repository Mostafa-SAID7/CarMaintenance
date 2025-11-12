using MediatR;

namespace CommunityCar.Application.Features.Comments.Commands;

public class DeleteCommentCommand : IRequest<bool>
{
    public int CommentId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
}