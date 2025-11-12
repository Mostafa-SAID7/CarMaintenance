using CommunityCar.Domain.Entities.Community;

namespace CommunityCar.Application.DTOs.Community;

public class GroupMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public GroupMemberRole Role { get; set; }
    public GroupMemberStatus Status { get; set; }
    public DateTime JoinedAt { get; set; }
}
