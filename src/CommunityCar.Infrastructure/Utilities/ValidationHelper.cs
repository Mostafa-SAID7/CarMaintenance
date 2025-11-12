using CommunityCar.Domain.Entities.Community;
using CommunityCar.Domain.Interfaces;

namespace CommunityCar.Infrastructure;

public static class ValidationHelper
{
    public static async Task<Post> ValidatePostExistsAndNotLockedAsync(IRepository<Post> postRepository, int postId)
    {
        var post = await postRepository.GetByIdAsync(postId);
        if (post == null || post.IsDeleted || post.IsLocked)
            throw new ArgumentException("Invalid or locked post");
        return post;
    }

    public static async Task<Forum> ValidateForumExistsAndActiveAsync(IRepository<Forum> forumRepository, int forumId)
    {
        var forum = await forumRepository.GetByIdAsync(forumId);
        if (forum == null || !forum.IsActive)
            throw new ArgumentException("Invalid or inactive forum");
        return forum;
    }
}