using CommunityCar.Application.DTOs.Community;
using CommunityCar.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommunityCar.Application.Features.Conversations.Queries;

public class GetUserConversationsQueryHandler : IRequestHandler<GetUserConversationsQuery, IEnumerable<ConversationDto>>
{
    private readonly IRepository<Domain.Entities.Community.Conversation> _conversationRepository;

    public GetUserConversationsQueryHandler(IRepository<Domain.Entities.Community.Conversation> conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<IEnumerable<ConversationDto>> Handle(GetUserConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await _conversationRepository.GetAll()
            .Where(c => !c.IsDeleted)
            .Include(c => c.Participants.Where(p => p.UserId == request.UserId))
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .Where(c => c.Participants.Any(p => p.UserId == request.UserId))
            .Select(c => new ConversationDto
            {
                Id = c.Id,
                Title = c.Title,
                Type = c.Type,
                LastMessagePreview = c.Messages.FirstOrDefault() != null ? 
                    (c.Messages.First().Content.Length > 50 ? 
                        c.Messages.First().Content.Substring(0, 50) + "..." : 
                        c.Messages.First().Content) : null,
                LastMessageAt = c.Messages.FirstOrDefault()?.CreatedAt,
                UnreadCount = c.Messages.Count(m => !m.IsRead && m.SenderId != request.UserId),
                Participants = c.Participants.Select(p => new ConversationParticipantDto
                {
                    UserId = p.UserId,
                    UserName = p.User.FirstName + " " + p.User.LastName,
                    Role = p.Role,
                    LastReadAt = p.LastReadAt
                }).ToList()
            })
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken);

        return conversations;
    }
}
