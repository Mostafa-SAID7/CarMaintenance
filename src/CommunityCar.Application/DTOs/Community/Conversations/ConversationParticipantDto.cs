using CommunityCar.Domain.Entities.Community;

namespace CommunityCar.Application.DTOs.Community;

public class ConversationParticipantDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public ConversationParticipantRole Role { get; set; }
    public DateTime? LastReadAt { get; set; }
}
