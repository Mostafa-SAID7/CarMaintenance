using MediatR;

namespace CommunityCar.Application.Features.Groups.Commands;

public class JoinGroupCommand : IRequest<bool>
{
    public int GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
}