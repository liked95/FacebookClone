using FacebookClone.Models.DomainModels;
using FacebookClone.Models.DTOs;

namespace FacebookClone.Services.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<PostResponseDto>> GetPostsByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<PostResponseDto?> CreatePostAsync(Guid userId, CreatePostDto post);
        Task<PostResponseDto?> UpdatePostAsync(Guid id, UpdatePostDto post);
        Task<bool> DeletePostAsync(Guid postId);
        Task<bool> IsPostOwnerAsync(Guid postId, Guid userId);
    }
}
