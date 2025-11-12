using MediatR;

namespace CommunityCar.Application.Features.Comments.Commands;

public class AcceptAnswerCommand : IRequest<bool>
{
    public int CommentId { get; set; }
    public string PostAuthorId { get; set; } = string.Empty;
}