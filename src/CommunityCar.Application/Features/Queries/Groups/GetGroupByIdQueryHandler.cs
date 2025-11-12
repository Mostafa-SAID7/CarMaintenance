using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Groups.Queries;

public class GetGroupByIdQueryHandler : IRequestHandler<GetGroupByIdQuery, GroupDto?>
{
    private readonly IGroupService _groupService;

    public GetGroupByIdQueryHandler(IGroupService groupService)
    {
        _groupService = groupService;
    }

    public async Task<GroupDto?> Handle(GetGroupByIdQuery request, CancellationToken cancellationToken)
    {
        return await _groupService.GetGroupByIdAsync(request.GroupId);
    }
}
