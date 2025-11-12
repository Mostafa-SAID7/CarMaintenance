using CommunityCar.Application.Interfaces.Community;
using MediatR;

namespace CommunityCar.Application.Features.Posts.Commands;

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, bool>
{
    private readonly IPostService _postService;

    public DeletePostCommandHandler(IPostService postService)
    {
        _postService = postService;
    }

    public async Task<bool> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        return await _postService.DeletePostAsync(request.PostId, request.AuthorId);
    }
}
