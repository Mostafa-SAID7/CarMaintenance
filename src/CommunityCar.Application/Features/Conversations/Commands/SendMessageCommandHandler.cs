using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Conversations.Commands;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, int>
{
    private readonly IConversationService _conversationService;

    public SendMessageCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task<int> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var sendRequest = new SendMessageRequest
        {
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            Content = request.Content,
            MessageType = request.MessageType,
            AttachmentUrl = request.AttachmentUrl,
            ReplyToMessageId = request.ReplyToMessageId
        };

        return await _conversationService.SendMessageAsync(sendRequest);
    }
}