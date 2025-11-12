using CommunityCar.Application.DTOs.Community;
using MediatR;

namespace CommunityCar.Application.Features.Groups.Queries;

public class GetGroupByIdQuery : IRequest<GroupDto?>
{
    public int GroupId { get; set; }
}
