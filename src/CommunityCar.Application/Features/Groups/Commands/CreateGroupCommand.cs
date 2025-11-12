using MediatR;

namespace CommunityCar.Application.Features.Groups.Commands;

public class CreateGroupCommand : IRequest<int>
{
    public string OwnerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Domain.Entities.Community.GroupPrivacy Privacy { get; set; }
    public string? CoverImageUrl { get; set; }
}