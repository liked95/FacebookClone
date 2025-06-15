using FacebookClone.Models.DomainModels;
using FacebookClone.Models.DTOs;
using FacebookClone.Repositories.Implementations;
using FacebookClone.Repositories.Interfaces;
using FacebookClone.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FacebookClone.Services.Implementations
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PostService> _logger;

        public PostService(IPostRepository postRepository, 
            ICommentRepository commentRepository,
            ILikeRepository likeRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PostService> logger)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _likeRepository = likeRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        public async Task<IEnumerable<PostResponseDto>> GetPostsByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            var posts = await _postRepository.GetAllPostsByUserIdAsync(userId, pageNumber, pageSize);
            var result = new List<PostResponseDto>();
            foreach( var post in posts)
            {
                result.Add(await MapToPostResponseDto(post));
            }
            return result;
        }



        public async Task<PostResponseDto?> CreatePostAsync(Guid userId, CreatePostDto createPostDto)
        {
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Content = createPostDto.Content,
                UserId = userId,
                Privacy = createPostDto.Privacy,
                ImageUrl = createPostDto.ImageUrl,
                VideoUrl = createPostDto.VideoUrl,
                FileUrl = createPostDto.FileUrl,
                CreatedAt = DateTime.UtcNow
            };

            var createdPost = await _postRepository.CreatePostAsync(post);
            if (createdPost == null)
            {
                _logger.LogError("Failed to create post for user: {UserId}", userId);
                return null;
            }

            return await MapToPostResponseDto(createdPost);
        }

        public async Task<PostResponseDto?> UpdatePostAsync(Guid id, UpdatePostDto updatePostDto)
        {
            var existingPost = await _postRepository.GetByIdAsync(id);
            if (existingPost == null)
                return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updatePostDto.Content))
                existingPost.Content = updatePostDto.Content;

            if (updatePostDto.Privacy.HasValue)
                existingPost.Privacy = updatePostDto.Privacy.Value;

            if (updatePostDto.ImageUrl != null)
                existingPost.ImageUrl = updatePostDto.ImageUrl;

            if (updatePostDto.VideoUrl != null)
                existingPost.VideoUrl = updatePostDto.VideoUrl;

            if (updatePostDto.FileUrl != null)
                existingPost.FileUrl = updatePostDto.FileUrl;

            var updatedPost = await _postRepository.UpdatePostAsync(existingPost);
            if (updatedPost == null)
            {
                _logger.LogError("Failed to update post with ID: {PostId}", id);
                return null;
            }

            _logger.LogInformation("Post updated successfully with ID: {PostId}", updatedPost.Id);
            return await MapToPostResponseDto(updatedPost);
        }

        public async Task<bool> DeletePostAsync(Guid id)
        {
            var result = await _postRepository.DeletePostAsync(id);
            if (result)
            {
                _logger.LogInformation("Post deleted successfully with ID: {PostId}", id);
            }
            return result;
        }


        public async Task<bool> IsPostOwnerAsync(Guid postId, Guid userId)
        {
            return await _postRepository.IsPostOwnerAsync(postId, userId);
        }

        public async Task<IEnumerable<PostResponseDto>> GetPostsForFeed(int pageNumber, int pageSize)
        {
            var posts = await _postRepository.GetAllPostsForFeed(pageNumber, pageSize);
            var result = new List<PostResponseDto>();

            foreach (var post in posts)
            {
                result.Add(await MapToPostResponseDto(post));
            }
            return result;
        }

        // Helper method to get current user ID
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private async Task<PostResponseDto> MapToPostResponseDto(Post post)
        {
            var commentsCount = await _commentRepository.GetPostCommentsCountAsync(post.Id);
            var likesCount = await _likeRepository.GetPostLikesCountAsync(post.Id);

            var currentUserId = GetCurrentUserId();
            var isLikedByCurrentUser = currentUserId.HasValue &&
                await _likeRepository.IsPostLikedByUserAsync(post.Id, currentUserId.Value);

            return new PostResponseDto
            {
                Id = post.Id,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UserId = post.UserId ?? Guid.Empty,
                Username = post.User?.Username ?? string.Empty,
                UserAvatarUrl = post.User?.AvatarUrl,
                Privacy = post.Privacy,
                IsEdited = post.IsEdited,
                ImageUrl = post.ImageUrl,
                VideoUrl = post.VideoUrl,
                FileUrl = post.FileUrl,
                CommentsCount = commentsCount,
                LikesCount = likesCount,
                IsLikedByCurrentUser = isLikedByCurrentUser

            };
        }
    }
}
