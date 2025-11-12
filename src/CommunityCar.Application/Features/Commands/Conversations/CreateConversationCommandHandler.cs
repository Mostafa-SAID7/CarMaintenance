using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Conversations.Commands;

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, int>
{
    private readonly IConversationService _conversationService;

    public CreateConversationCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task<int> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new CreateConversationRequest
        {
            CreatorId = request.CreatorId,
            Title = request.Title,
            Type = request.Type,
            ParticipantIds = request.ParticipantIds
        };

        return await _conversationService.CreateConversationAsync(createRequest);
    }
}
