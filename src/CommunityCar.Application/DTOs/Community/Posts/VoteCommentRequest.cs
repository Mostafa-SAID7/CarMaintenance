using CommunityCar.Domain.Enums;

namespace CommunityCar.Application.DTOs.Community;

public class VoteCommentRequest
{
    public VoteType VoteType { get; set; }
}
