using CommunityCar.Application.Interfaces.Commuinty;
using MediatR;

namespace CommunityCar.Application.Features.Comments.Commands;

public class AcceptAnswerCommandHandler : IRequestHandler<AcceptAnswerCommand, bool>
{
    private readonly ICommentService _commentService;

    public AcceptAnswerCommandHandler(ICommentService commentService)
    {
        _commentService = commentService;
    }

    public async Task<bool> Handle(AcceptAnswerCommand request, CancellationToken cancellationToken)
    {
        return await _commentService.AcceptAnswerAsync(request.CommentId, request.PostAuthorId);
    }
}