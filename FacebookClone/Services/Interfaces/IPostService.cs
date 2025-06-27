using FacebookClone.Models.DomainModels;
using FacebookClone.Models.DTOs;

namespace FacebookClone.Services.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<PostResponseDto>> GetPostsByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<PostResponseDto?> CreatePostWithMediaAsync(Guid userId, CreatePostDto createPostDto, List<IFormFile>? mediaFiles = null);
        Task<PostResponseDto?> UpdatePostWithMediaAsync(Guid userId, Guid postId, UpdatePostDto post, List<IFormFile>? mediaFiles = null);
        Task<bool> DeletePostAsync(Guid postId);
        Task<bool> IsPostOwnerAsync(Guid postId, Guid userId);
        Task<IEnumerable<PostResponseDto>> GetPostsForFeed(int pageNumber, int pageSize);
    }
}
