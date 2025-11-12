using CommunityCar.Application.Interfaces.Community;
using MediatR;

namespace CommunityCar.Application.Features.Comments.Commands;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, bool>
{
    private readonly ICommentService _commentService;

    public DeleteCommentCommandHandler(ICommentService commentService)
    {
        _commentService = commentService;
    }

    public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        return await _commentService.DeleteCommentAsync(request.CommentId, request.AuthorId);
    }
}
