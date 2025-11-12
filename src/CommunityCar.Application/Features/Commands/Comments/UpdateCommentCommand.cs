using MediatR;

namespace CommunityCar.Application.Features.Comments.Commands;

public class UpdateCommentCommand : IRequest<bool>
{
    public int CommentId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
