using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Groups.Commands;

public class JoinGroupCommandHandler : IRequestHandler<JoinGroupCommand, bool>
{
    private readonly IGroupService _groupService;

    public JoinGroupCommandHandler(IGroupService groupService)
    {
        _groupService = groupService;
    }

    public async Task<bool> Handle(JoinGroupCommand request, CancellationToken cancellationToken)
    {
        return await _groupService.JoinGroupAsync(request.GroupId, request.UserId);
    }
}