using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Posts.Commands;

public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, bool>
{
    private readonly IPostService _postService;

    public UpdatePostCommandHandler(IPostService postService)
    {
        _postService = postService;
    }

    public async Task<bool> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdatePostRequest
        {
            Title = request.Title,
            Content = request.Content,
            Tags = request.Tags
        };

        return await _postService.UpdatePostAsync(request.PostId, request.AuthorId, updateRequest);
    }
}