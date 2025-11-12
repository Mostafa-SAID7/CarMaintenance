using CommunityCar.Application.DTOs.Community;
using CommunityCar.Domain.Enums;

namespace CommunityCar.Application.Interfaces.Community;

public interface IPostService
{
    Task<int> CreatePostAsync(CreatePostRequest request, string authorId);
    Task<PostDto?> GetPostByIdAsync(int postId, string? userId = null);
    Task<IEnumerable<PostDto>> GetPostsByForumAsync(int forumId, int page = 1, int pageSize = 20);
    Task<IEnumerable<PostDto>> GetPostsByUserAsync(string userId, int page = 1, int pageSize = 20);
    Task<bool> VotePostAsync(int postId, string userId, VoteType voteType);
    Task<bool> UpdatePostAsync(int postId, string authorId, UpdatePostRequest request);
    Task<bool> DeletePostAsync(int postId, string authorId);
    Task<bool> PinPostAsync(int postId, string moderatorId, bool pin);
    Task<bool> LockPostAsync(int postId, string moderatorId, bool @lock);
}
