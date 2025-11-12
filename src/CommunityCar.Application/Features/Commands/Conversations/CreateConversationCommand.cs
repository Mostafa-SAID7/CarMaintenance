using MediatR;

namespace CommunityCar.Application.Features.Conversations.Commands;

public class CreateConversationCommand : IRequest<int>
{
    public string CreatorId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Domain.Entities.Community.ConversationType Type { get; set; }
    public IEnumerable<string> ParticipantIds { get; set; } = new List<string>();
}
