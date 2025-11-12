namespace CommunityCar.Application.DTOs.Community;

public class SendMessageRequest
{
    public string Content { get; set; } = string.Empty;
    public string? MessageType { get; set; } = "Text";
    public string? AttachmentUrl { get; set; }
    public int? ReplyToMessageId { get; set; }
}
