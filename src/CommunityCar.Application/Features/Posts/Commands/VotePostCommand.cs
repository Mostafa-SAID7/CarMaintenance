using MediatR;

namespace CommunityCar.Application.Features.Posts.Commands;

public class VotePostCommand : IRequest<bool>
{
    public int PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Domain.Entities.Community.VoteType VoteType { get; set; }
}