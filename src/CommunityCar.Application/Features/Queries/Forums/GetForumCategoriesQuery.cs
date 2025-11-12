using CommunityCar.Application.DTOs.Community;
using MediatR;

namespace CommunityCar.Application.Features.Forums.Queries;

public class GetForumCategoriesQuery : IRequest<IEnumerable<ForumCategoryDto>>
{
}
