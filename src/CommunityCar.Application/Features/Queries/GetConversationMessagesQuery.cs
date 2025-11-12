using CommunityCar.Application.DTOs.Community;
using MediatR;

namespace CommunityCar.Application.Features.Conversations.Queries;

public class GetConversationMessagesQuery : IRequest<IEnumerable<MessageDto>>
{
    public int ConversationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}