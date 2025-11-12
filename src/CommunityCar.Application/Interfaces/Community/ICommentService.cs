using CommunityCar.Application.DTOs.Community;
using CommunityCar.Domain.Enums;

namespace CommunityCar.Application.Interfaces.Community;

public interface ICommentService
{
    Task<int> CreateCommentAsync(CreateCommentRequest request, string authorId);
    Task<CommentDto?> GetCommentByIdAsync(int commentId);
    Task<IEnumerable<CommentDto>> GetCommentsByPostAsync(int postId, int page = 1, int pageSize = 20);
    Task<bool> VoteCommentAsync(int commentId, string userId, VoteType voteType);
    Task<bool> UpdateCommentAsync(int commentId, string authorId, UpdateCommentRequest request);
    Task<bool> DeleteCommentAsync(int commentId, string authorId);
    Task<bool> AcceptAnswerAsync(int commentId, string postAuthorId);
}
