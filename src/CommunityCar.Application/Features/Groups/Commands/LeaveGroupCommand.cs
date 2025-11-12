using MediatR;

namespace CommunityCar.Application.Features.Groups.Commands;

public class LeaveGroupCommand : IRequest<bool>
{
    public int GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
}