using MediatR;

namespace CommunityCar.Application.Features.Conversations.Commands;

public class SendMessageCommand : IRequest<int>
{
    public int ConversationId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? MessageType { get; set; } = "Text";
    public string? AttachmentUrl { get; set; }
    public int? ReplyToMessageId { get; set; }
}
