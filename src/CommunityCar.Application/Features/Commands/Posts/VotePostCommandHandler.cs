using CommunityCar.Application.Interfaces.Commuinty;
using MediatR;

namespace CommunityCar.Application.Features.Posts.Commands;

public class VotePostCommandHandler : IRequestHandler<VotePostCommand, bool>
{
    private readonly IPostService _postService;

    public VotePostCommandHandler(IPostService postService)
    {
        _postService = postService;
    }

    public async Task<bool> Handle(VotePostCommand request, CancellationToken cancellationToken)
    {
        return await _postService.VotePostAsync(request.PostId, request.UserId, request.VoteType);
    }
}