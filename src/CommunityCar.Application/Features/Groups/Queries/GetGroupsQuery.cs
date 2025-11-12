using CommunityCar.Application.DTOs.Community;
using MediatR;

namespace CommunityCar.Application.Features.Groups.Queries;

public class GetGroupsQuery : IRequest<IEnumerable<GroupDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public Domain.Entities.Community.GroupPrivacy? Privacy { get; set; }
}