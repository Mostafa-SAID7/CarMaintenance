using CommunityCar.Domain.Enums;
using MediatR;

namespace CommunityCar.Application.Features.Comments.Commands;

public class VoteCommentCommand : IRequest<bool>
{
    public int CommentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public VoteType VoteType { get; set; }
}
