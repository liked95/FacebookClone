using FacebookClone.Models.DomainModels;

namespace FacebookClone.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task<Post?> GetByIdAsync(Guid id);
        Task<IEnumerable<Post>> GetAllPostsByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<IEnumerable<Post>> GetAllPostsForFeed(int pageNumber, int pageSize);
        Task<Post?> CreatePostAsync(Post post);
        Task<Post?> UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(Guid id);
        Task<bool> IsPostOwnerAsync(Guid postId, Guid userId);
    }
}
