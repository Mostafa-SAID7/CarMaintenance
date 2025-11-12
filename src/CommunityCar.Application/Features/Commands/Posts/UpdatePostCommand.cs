using MediatR;

namespace CommunityCar.Application.Features.Posts.Commands;

public class UpdatePostCommand : IRequest<bool>
{
    public int PostId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Tags { get; set; }
}
