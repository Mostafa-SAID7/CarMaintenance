using CommunityCar.Domain.Enums;
using MediatR;

namespace CommunityCar.Application.Features.Posts.Commands;

public class VotePostCommand : IRequest<bool>
{
    public int PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public VoteType VoteType { get; set; }
}
