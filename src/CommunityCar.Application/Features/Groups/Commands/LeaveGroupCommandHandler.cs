using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Groups.Commands;

public class LeaveGroupCommandHandler : IRequestHandler<LeaveGroupCommand, bool>
{
    private readonly IGroupService _groupService;

    public LeaveGroupCommandHandler(IGroupService groupService)
    {
        _groupService = groupService;
    }

    public async Task<bool> Handle(LeaveGroupCommand request, CancellationToken cancellationToken)
    {
        return await _groupService.LeaveGroupAsync(request.GroupId, request.UserId);
    }
}