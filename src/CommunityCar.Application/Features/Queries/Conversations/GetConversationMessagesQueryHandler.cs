using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Conversations.Queries;

public class GetConversationMessagesQueryHandler : IRequestHandler<GetConversationMessagesQuery, IEnumerable<MessageDto>>
{
    private readonly IConversationService _conversationService;

    public GetConversationMessagesQueryHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task<IEnumerable<MessageDto>> Handle(GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        return await _conversationService.GetConversationMessagesAsync(request.ConversationId, request.UserId, request.Page, request.PageSize);
    }
}