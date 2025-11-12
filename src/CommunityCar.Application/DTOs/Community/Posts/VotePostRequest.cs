using CommunityCar.Domain.Entities.Community;

namespace CommunityCar.Application.DTOs.Community;

public class VotePostRequest
{
    public VoteType VoteType { get; set; }
}