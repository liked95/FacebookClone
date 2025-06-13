using FacebookClone.Models.DomainModels;
using FacebookClone.Models.DTOs;
using FacebookClone.Repositories.Interfaces;
using FacebookClone.Services.Interfaces;

namespace FacebookClone.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly ILogger<CommentService> _logger;

        public CommentService(ICommentRepository commentRepository,
            IPostRepository postRepository,
            ILogger<CommentService> logger)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _logger = logger;
        }

        public async Task<CommentResponseDto?> CreateCommentAsync(Guid userId, Guid postId, CreateCommentDto createCommentDto)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                throw new InvalidOperationException($"Post with ID {postId} does not exist.");
            }

            Comment comment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = createCommentDto.Content,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                PostId = post.Id,
            };

            var createdComment = await _commentRepository.CreateCommentAsync(comment);

            if (createdComment == null)
            {
                _logger.LogError("Failed to create comment for post: {PostId}", postId);
                return null;
            }

            _logger.LogInformation("Comment created successfully with ID: {CommentId}", createdComment.Id);
            return MapToCommentResponseDto(createdComment);
        }

        public async Task<bool> DeleteCommentAsync(Guid id)
        {
            var result = await _commentRepository.DeleteCommentAsync(id);
            if (result)
            {
                _logger.LogInformation("Comment deleted successfully with ID: {id}", id);
            }
            return result;
        }

        public async Task<CommentResponseDto?> GetCommentByIdAsync(Guid id)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return null;
            }

            return MapToCommentResponseDto(comment);
        }

        public async Task<IEnumerable<CommentResponseDto>> GetCommentsByPostIdAsync(Guid postId, int pageNumber, int pageSize)
        {
            var comments = await _commentRepository.GetCommentsByPostIdAsync(postId, pageNumber, pageSize);
            return comments.Select(MapToCommentResponseDto);
        }

        public async Task<bool> IsCommentOwnerAsync(Guid commentId, Guid userId)
        {
            return await _commentRepository.IsCommentOwnerAsync(commentId, userId);
        }

        public async Task<CommentResponseDto?> UpdateCommentAsync(Guid commentId, UpdateCommentDto updateCommentDto)
        {
            var existingComment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (existingComment == null) { 
                throw new InvalidOperationException($"Comment with ID {commentId} does not exist.");
            }

            if (!string.IsNullOrEmpty(updateCommentDto.Content))
            {
                existingComment.Content = updateCommentDto.Content;
            }

            var updatedComment = await _commentRepository.UpdateCommentAsync(existingComment);

            if (updatedComment == null)
            {
                _logger.LogError("Failed to update comment with ID: {CommentId}", commentId);
                return null;
            }

            _logger.LogInformation("Comment updated successfully with ID: {CommentId}", updatedComment.Id);
            return MapToCommentResponseDto(updatedComment);
        }

        private static CommentResponseDto MapToCommentResponseDto(Comment comment)
        {
            return new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                IsEdited = comment.IsEdited,
                UserId = comment.UserId,
                Username = comment.User?.Username ?? string.Empty,
                UserAvatarUrl = comment.User?.AvatarUrl,
                PostId = comment.PostId
            };
        }
    }
}
