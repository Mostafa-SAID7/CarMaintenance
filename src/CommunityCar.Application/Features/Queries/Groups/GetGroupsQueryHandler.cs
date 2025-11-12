using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.Interfaces;
using MediatR;

namespace CommunityCar.Application.Features.Groups.Queries;

public class GetGroupsQueryHandler : IRequestHandler<GetGroupsQuery, IEnumerable<GroupDto>>
{
    private readonly IGroupService _groupService;

    public GetGroupsQueryHandler(IGroupService groupService)
    {
        _groupService = groupService;
    }

    public async Task<IEnumerable<GroupDto>> Handle(GetGroupsQuery request, CancellationToken cancellationToken)
    {
        return await _groupService.GetGroupsAsync(request.SearchTerm, request.Privacy, request.Page, request.PageSize);
    }
}