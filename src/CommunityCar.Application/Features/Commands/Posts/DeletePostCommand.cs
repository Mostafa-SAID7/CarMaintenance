using MediatR;

namespace CommunityCar.Application.Features.Posts.Commands;

public class DeletePostCommand : IRequest<bool>
{
    public int PostId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
}