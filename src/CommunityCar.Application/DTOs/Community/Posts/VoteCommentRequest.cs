using CommunityCar.Domain.Entities.Community;

namespace CommunityCar.Application.DTOs.Community;

public class VoteCommentRequest
{
    public VoteType VoteType { get; set; }
}