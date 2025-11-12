using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Groups.Commands;

public class CreateGroupCommandHandler : IRequestHandler<CreateGroupCommand, int>
{
    private readonly IGroupService _groupService;

    public CreateGroupCommandHandler(IGroupService groupService)
    {
        _groupService = groupService;
    }

    public async Task<int> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new CreateGroupRequest
        {
            OwnerId = request.OwnerId,
            Name = request.Name,
            Description = request.Description,
            Privacy = request.Privacy,
            CoverImageUrl = request.CoverImageUrl
        };

        return await _groupService.CreateGroupAsync(createRequest);
    }
}
