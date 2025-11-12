using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Comments.Commands;

public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, bool>
{
    private readonly ICommentService _commentService;

    public UpdateCommentCommandHandler(ICommentService commentService)
    {
        _commentService = commentService;
    }

    public async Task<bool> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateCommentRequest
        {
            Content = request.Content
        };

        return await _commentService.UpdateCommentAsync(request.CommentId, request.AuthorId, updateRequest);
    }
}