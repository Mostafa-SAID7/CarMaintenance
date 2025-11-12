using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces.Community;
using MediatR;

namespace CommunityCar.Application.Features.Comments.Queries;

public class GetCommentsByPostQueryHandler : IRequestHandler<GetCommentsByPostQuery, IEnumerable<CommentDto>>
{
    private readonly ICommentService _commentService;

    public GetCommentsByPostQueryHandler(ICommentService commentService)
    {
        _commentService = commentService;
    }

    public async Task<IEnumerable<CommentDto>> Handle(GetCommentsByPostQuery request, CancellationToken cancellationToken)
    {
        return await _commentService.GetCommentsByPostAsync(request.PostId, request.Page, request.PageSize);
    }
}
