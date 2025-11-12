using CommunityCar.Application.DTOs.Community;
using MediatR;

namespace CommunityCar.Application.Features.Forums.Queries;

public class GetForumByIdQuery : IRequest<ForumDto?>
{
    public int ForumId { get; set; }
}
