using CommunityCar.Application.DTOs.Community;
using MediatR;

namespace CommunityCar.Application.Features.Conversations.Queries;

public class GetUserConversationsQuery : IRequest<IEnumerable<ConversationDto>>
{
    public string UserId { get; set; } = string.Empty;
}
