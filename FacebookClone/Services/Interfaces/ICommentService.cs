using FacebookClone.Models.DTOs;

namespace FacebookClone.Services.Interfaces
{
    public interface ICommentService
    {
        Task<CommentResponseDto?> GetCommentByIdAsync(Guid id);
        Task<IEnumerable<CommentResponseDto>> GetCommentsByPostIdAsync(Guid postId, int pageNumber, int pageSize, Guid? parentCommentId);
        Task<IEnumerable<CommentResponseDto>> GetRepliesForCommentAsync(Guid parentCommentId, int pageNumber, int pageSize);
        Task<CommentResponseDto?> CreateCommentAsync(Guid userId, Guid postId, CreateCommentDto createCommentDto);
        Task<CommentResponseDto?> UpdateCommentAsync(Guid commentId, UpdateCommentDto updateCommentDto);
        Task<bool> DeleteCommentAsync(Guid id);
        Task<bool> IsCommentOwnerAsync(Guid commentId, Guid userId);
    }
}
