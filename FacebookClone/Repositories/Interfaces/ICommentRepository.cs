using FacebookClone.Models.DomainModels;

namespace FacebookClone.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task<Comment?> GetCommentByIdAsync(Guid id);
        Task<Comment?> CreateCommentAsync(Comment comment);
        Task<Comment?> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(Guid id);
        Task<bool> IsCommentOwnerAsync(Guid commentId, Guid ownerId);
        Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId, int pageNumber, int pageSize);
        Task<int> GetPostCommentsCountAsync(Guid postId);
    }
}
