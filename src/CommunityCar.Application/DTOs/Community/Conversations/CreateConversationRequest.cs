using CommunityCar.Domain.Entities.Community;

namespace CommunityCar.Application.DTOs.Community;

public class CreateConversationRequest
{
    public string Title { get; set; } = string.Empty;
    public ConversationType Type { get; set; }
    public IEnumerable<string> ParticipantIds { get; set; } = new List<string>();
}