namespace CommunityCar.Application.DTOs.Community;

public class MessageDto
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? MessageType { get; set; }
    public string? AttachmentUrl { get; set; }
    public int? ReplyToMessageId { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}
