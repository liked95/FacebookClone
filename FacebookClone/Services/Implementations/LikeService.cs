using FacebookClone.Models.DTOs;
using FacebookClone.Repositories.Interfaces;
using FacebookClone.Services.Interfaces;

namespace FacebookClone.Services.Implementations
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly ILogger<LikeService> _logger;

        public LikeService(
            ILikeRepository likeRepository,
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            ILogger<LikeService> logger)
        {
            _likeRepository = likeRepository;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _logger = logger;
        }

        public async Task<LikeActionResult> TogglePostLikeAsync(Guid postId, Guid userId)
        {
            // Verify post exists
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                throw new InvalidOperationException($"Post with ID {postId} does not exist.");
            }

            var result = await _likeRepository.TogglePostLikeAsync(postId, userId);

            _logger.LogInformation("User {UserId} {Action} post {PostId}",
                userId, result.IsLiked ? "liked" : "unliked", postId);

            return result;
        }

        public async Task<IEnumerable<PostLikeDto>> GetPostLikesAsync(Guid postId, int pageNumber, int pageSize)
        {
            return await _likeRepository.GetPostLikesAsync(postId, pageNumber, pageSize);
        }

        public async Task<LikeActionResult> ToggleCommentLikeAsync(Guid commentId, Guid userId)
        {
            // Verify comment exists
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                throw new InvalidOperationException($"Comment with ID {commentId} does not exist.");
            }

            var result = await _likeRepository.ToggleCommentLikeAsync(commentId, userId);

            _logger.LogInformation("User {UserId} {Action} comment {CommentId}",
                userId, result.IsLiked ? "liked" : "unliked", commentId);

            return result;
        }

        public async Task<IEnumerable<CommentLikeDto>> GetCommentLikesAsync(Guid commentId, int pageNumber, int pageSize)
        {
            return await _likeRepository.GetCommentLikesAsync(commentId, pageNumber, pageSize);
        }
    }
}
