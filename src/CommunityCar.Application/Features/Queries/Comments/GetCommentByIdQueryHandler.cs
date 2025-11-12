using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces.Community;
using MediatR;

namespace CommunityCar.Application.Features.Comments.Queries;

public class GetCommentByIdQueryHandler : IRequestHandler<GetCommentByIdQuery, CommentDto?>
{
    private readonly ICommentService _commentService;

    public GetCommentByIdQueryHandler(ICommentService commentService)
    {
        _commentService = commentService;
    }

    public async Task<CommentDto?> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _commentService.GetCommentByIdAsync(request.CommentId);
    }
}
