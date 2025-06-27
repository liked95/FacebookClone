using FacebookClone.Models.Constants;
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
        private readonly IMediaService _mediaService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PostService> _logger;

        public PostService(IPostRepository postRepository,
            ICommentRepository commentRepository,
            ILikeRepository likeRepository,
            IMediaService mediaService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PostService> logger)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _likeRepository = likeRepository;
            _mediaService = mediaService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        public async Task<IEnumerable<PostResponseDto>> GetPostsByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            var posts = await _postRepository.GetAllPostsByUserIdAsync(userId, pageNumber, pageSize);
            var result = new List<PostResponseDto>();
            foreach (var post in posts)
            {
                result.Add(await MapToPostResponseDto(post));
            }
            return result;
        }

        public async Task<PostResponseDto?> CreatePostWithMediaAsync(
            Guid userId,
            CreatePostDto createPostDto,
            List<IFormFile>? mediaFiles = null
        )
        {
            try
            {
                var post = new Post
                {
                    Id = Guid.NewGuid(),
                    Content = createPostDto.Content,
                    UserId = userId,
                    Privacy = createPostDto.Privacy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdPost = await _postRepository.CreatePostAsync(post);

                if (createdPost == null)
                {
                    _logger.LogError("Failed to create post for user: {UserId}", userId);
                    return null;
                }

                // Upload media files if provided
                if (mediaFiles?.Any() == true)
                {
                    await _mediaService.UploadMultipleFilesAsync(
                        userId,
                        mediaFiles,
                        MediaAttachmentType.Post,
                        post.Id.ToString());
                }

                return await MapToPostResponseDto(createdPost);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post with media for user {UserId}", userId);
                throw;
            }
        }

        public async Task<PostResponseDto?> UpdatePostWithMediaAsync(
            Guid userId,
            Guid postId,
            UpdatePostDto updatePostDto,
            List<IFormFile>? mediaFiles = null)
        {
            var existingPost = await _postRepository.GetByIdAsync(postId);
            if (existingPost == null)
                return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updatePostDto.Content))
                existingPost.Content = updatePostDto.Content;

            if (updatePostDto.Privacy.HasValue)
                existingPost.Privacy = updatePostDto.Privacy.Value;


            var updatedPost = await _postRepository.UpdatePostAsync(existingPost);
            if (updatedPost == null)
            {
                _logger.LogError("Failed to update post with ID: {PostId}", postId);
                return null;
            }

            _logger.LogInformation("Post updated successfully with ID: {PostId}", updatedPost.Id);

            // Get all current media for the post
            var allMedias = await _mediaService.GetMediaFilesByAttachmentAsync(MediaAttachmentType.Post, postId.ToString());


            // Delete removed media
            var mediaToDeletes = allMedias.Where(m => !updatePostDto.ExistingMediaIds.Contains(m.Id)).ToList();
            var mediaToDeleteIds = mediaToDeletes.Select(m => m.Id).ToList();
            await _mediaService.DeleteMediaFilesByIdsAsync(mediaToDeleteIds);

           
            // Upload new medias
            if (mediaFiles?.Any() == true)
            {
                await _mediaService.UploadMultipleFilesAsync(userId, mediaFiles, MediaAttachmentType.Post, postId.ToString());
                _logger.LogInformation("Post {PostId} updated with new media files", postId);
            }

            return await MapToPostResponseDto(updatedPost);

        }

        public async Task<bool> DeletePostAsync(Guid id)
        {
            var existingPost = await _postRepository.GetByIdAsync(id);
            if (existingPost == null) return false;

            var deleteMediaResult = await _mediaService.DeleteMediaFilesByAttachmentAsync(MediaAttachmentType.Post, id.ToString());


            var result = await _postRepository.DeletePostAsync(id);
            if (result && deleteMediaResult)
            {
                _logger.LogInformation("Post deleted successfully with ID: {PostId}", id);
            }
            return result && deleteMediaResult;
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

            var mediaFiles = await _mediaService.GetMediaFilesByAttachmentAsync(MediaAttachmentType.Post, post.Id.ToString());

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
                CommentsCount = commentsCount,
                LikesCount = likesCount,
                IsLikedByCurrentUser = isLikedByCurrentUser,
                MediaFiles = mediaFiles.OrderBy(m => m.DisplayOrder).ToList()
            };
        }
    }
}
