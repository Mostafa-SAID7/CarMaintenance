using CommunityCar.Application.DTOs.Chat;

namespace CommunityCar.Application.Interfaces.Hub;

public interface IChatService
{
    Task<int> SendMessageAsync(SendChatMessageRequest request);
    Task MarkMessageAsReadAsync(int messageId, string userId);
    Task<IEnumerable<ChatMessageDto>> GetConversationAsync(string userId1, string userId2);
    Task<IEnumerable<ChatMessageDto>> GetUserMessagesAsync(string userId, bool onlyUnread = false);
    Task<int> GetUnreadCountAsync(string userId);
}