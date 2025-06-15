using FacebookClone.Models.DTOs;

namespace FacebookClone.Services.Interfaces
{
    public interface ILikeService
    {
        // Post Likes
        Task<LikeActionResult> TogglePostLikeAsync(Guid postId, Guid userId);
        Task<IEnumerable<PostLikeDto>> GetPostLikesAsync(Guid postId, int pageNumber, int pageSize);

        // Comment Likes
        Task<LikeActionResult> ToggleCommentLikeAsync(Guid commentId, Guid userId);
        Task<IEnumerable<CommentLikeDto>> GetCommentLikesAsync(Guid commentId, int pageNumber, int pageSize);
    }
}
