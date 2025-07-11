﻿using FacebookClone.Models.DomainModels;
using FacebookClone.Models.DTOs;
using FacebookClone.Repositories.Interfaces;
using FacebookClone.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FacebookClone.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CommentService> _logger;

        public CommentService(ICommentRepository commentRepository,
            IPostRepository postRepository,
            ILikeRepository likeRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CommentService> logger)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _likeRepository = likeRepository;
            _httpContextAccessor = httpContextAccessor;
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
                ParentCommentId = createCommentDto.ParentCommentId
            };

            // Calculate level
            if (createCommentDto.ParentCommentId.HasValue)
            {
                var parent = await GetCommentByIdAsync(createCommentDto.ParentCommentId.Value);
                if (parent == null || parent.PostId != post.Id)
                {
                    throw new InvalidOperationException("Invalid parent comment");
                }

                comment.Level = parent.Level + 1;
            }

            var createdComment = await _commentRepository.CreateCommentAsync(comment);

            if (createdComment == null)
            {
                _logger.LogError("Failed to create comment for post: {PostId}", postId);
                return null;
            }

            _logger.LogInformation("Comment created successfully with ID: {CommentId}", createdComment.Id);
            return await MapToCommentResponseDto(createdComment);
        }

        public async Task<bool> DeleteCommentAsync(Guid id)
        {
            var result = await _commentRepository.DeleteCommentAsync(id);
            if (result)
            {
                _logger.LogInformation("Comment and its replies deleted successfully with ID: {Id}", id);
            }
            else
            {
                _logger.LogWarning("Failed to delete comment with ID: {Id}", id);
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

            return await MapToCommentResponseDto(comment);
        }

        public async Task<IEnumerable<CommentResponseDto>> GetCommentsByPostIdAsync(Guid postId, int pageNumber, int pageSize, Guid? parentCommentId = null)
        {
            var comments = await _commentRepository.GetCommentsByPostIdAsync(postId, pageNumber, pageSize, parentCommentId);
            var result = new List<CommentResponseDto>();

            foreach (var comment in comments)
            {
                var commentResponseDto = await MapToCommentResponseDto(comment);
                result.Add(commentResponseDto);
            }

            return result;
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
            return await MapToCommentResponseDto(updatedComment);
        }

        public async Task<IEnumerable<CommentResponseDto>> GetRepliesForCommentAsync(Guid parentCommentId, int pageNumber, int pageSize)
        {
            var comment = await GetCommentByIdAsync(parentCommentId);
            if (comment == null)
            {
                throw new InvalidOperationException("Invalid parent comment");
            }


            return await GetCommentsByPostIdAsync(comment.PostId, pageNumber, pageSize, parentCommentId);
        }

        // Helper method to get current user ID
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private  async Task<CommentResponseDto> MapToCommentResponseDto(Comment comment)
        {
            var likesCount = await _likeRepository.GetCommentLikesCountAsync(comment.Id);
            var currentUserId = GetCurrentUserId();
            var isLikedByCurrentUser = currentUserId.HasValue &&
                await _likeRepository.IsCommentLikedByUserAsync(comment.Id, currentUserId.Value);
            var replyCount = await _commentRepository.GetReplyCountByCommentIdAsync(comment.Id);

            return new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                IsEdited = comment.IsEdited,
                Level = comment.Level,
                ParentCommentId = comment.ParentCommentId,
                ReplyCount = replyCount,
                UserId = comment.UserId ?? Guid.Empty,
                Username = comment.User?.Username ?? string.Empty,
                UserAvatarUrl = comment.User?.AvatarUrl,
                PostId = comment.PostId,
                LikesCount = likesCount,
                IsLikedByCurrentUser = isLikedByCurrentUser
            };
        }
    }
}
