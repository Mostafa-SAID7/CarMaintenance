using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Comments.Commands;

public class VoteCommentCommandHandler : IRequestHandler<VoteCommentCommand, bool>
{
    private readonly ICommentService _commentService;

    public VoteCommentCommandHandler(ICommentService commentService)
    {
        _commentService = commentService;
    }

    public async Task<bool> Handle(VoteCommentCommand request, CancellationToken cancellationToken)
    {
        return await _commentService.VoteCommentAsync(request.CommentId, request.UserId, request.VoteType);
    }
}