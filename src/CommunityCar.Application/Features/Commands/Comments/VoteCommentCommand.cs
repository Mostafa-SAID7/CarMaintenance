using MediatR;

namespace CommunityCar.Application.Features.Comments.Commands;

public class VoteCommentCommand : IRequest<bool>
{
    public int CommentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Domain.Entities.Community.VoteType VoteType { get; set; }
}