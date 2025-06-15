using FacebookClone.Models.DomainModels;
using FacebookClone.Models.DTOs;

namespace FacebookClone.Repositories.Interfaces
{
    public interface ILikeRepository
    {
        // Post Likes
        Task<LikeActionResult> TogglePostLikeAsync(Guid postId, Guid userId);
        Task<IEnumerable<PostLikeDto>> GetPostLikesAsync(Guid postId, int pageNumber, int pageSize);
        Task<int> GetPostLikesCountAsync(Guid postId);
        Task<bool> IsPostLikedByUserAsync(Guid postId, Guid userId);

        // Comment Likes
        Task<LikeActionResult> ToggleCommentLikeAsync(Guid commentId, Guid userId);
        Task<IEnumerable<CommentLikeDto>> GetCommentLikesAsync(Guid commentId, int pageNumber, int pageSize);
        Task<int> GetCommentLikesCountAsync(Guid commentId);
        Task<bool> IsCommentLikedByUserAsync(Guid commentId, Guid userId);
    }
}
