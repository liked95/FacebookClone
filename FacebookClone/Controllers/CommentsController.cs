using FacebookClone.Common;
using FacebookClone.Models.DTOs;
using FacebookClone.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FacebookClone.Controllers
{
    [ApiController]
    [Route("Api/Posts/{postId:guid}/comments")]
    [Produces("application/json")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(ICommentService commentService, ILogger<CommentsController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CommentResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CommentResponseDto>>>> GetPostComments(
            Guid postId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            try
            {
                var comments = await _commentService.GetCommentsByPostIdAsync(postId, pageNumber, pageSize);
                return Ok(ApiResponse<IEnumerable<CommentResponseDto>>.SuccessResponse(comments, "Comments fetched"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments for post {PostId}", postId);
                return StatusCode(500, ApiResponse<IEnumerable<CommentResponseDto>>.ErrorResponse("Internal server error", 500));
            }
        }

        [HttpGet("{commentId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CommentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CommentResponseDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<CommentResponseDto>>> GetComment(Guid postId, Guid commentId)
        {
            try
            {
                var comment = await _commentService.GetCommentByIdAsync(commentId);
                if (comment == null || comment.PostId != postId)
                    return NotFound(ApiResponse<CommentResponseDto>.ErrorResponse($"Comment with ID {commentId} not found in post {postId}", 404));

                return Ok(ApiResponse<CommentResponseDto>.SuccessResponse(comment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comment {CommentId} from post {PostId}", commentId, postId);
                return StatusCode(500, ApiResponse<CommentResponseDto>.ErrorResponse("Internal server error", 500));
            }
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CommentResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<CommentResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<CommentResponseDto>>> CreateComment(Guid postId, [FromBody] CreateCommentDto createCommentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<CommentResponseDto>.ErrorResponse("Validation failed", 400, errors));
                }

                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(ApiResponse<CommentResponseDto>.ErrorResponse("Unauthorized", 401));
                }

                var createdComment = await _commentService.CreateCommentAsync(Guid.Parse(currentUserId), postId, createCommentDto);
                if (createdComment == null)
                {
                    return BadRequest(ApiResponse<CommentResponseDto>.ErrorResponse("Failed to create comment"));
                }

                return CreatedAtAction(nameof(GetComment),
                    new { postId = postId, commentId = createdComment.Id },
                    ApiResponse<CommentResponseDto>.SuccessResponse(createdComment, "Comment created", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment on post {PostId}", postId);
                return StatusCode(500, ApiResponse<CommentResponseDto>.ErrorResponse("Internal server error", 500));
            }
        }

        [HttpPut("{commentId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CommentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CommentResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<CommentResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<CommentResponseDto>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<CommentResponseDto>>> UpdateComment(Guid postId, Guid commentId, [FromBody] UpdateCommentDto updateCommentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<CommentResponseDto>.ErrorResponse("Validation failed", 400, errors));
                }

                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(ApiResponse<CommentResponseDto>.ErrorResponse("Unauthorized", 401));
                }

                var existingComment = await _commentService.GetCommentByIdAsync(commentId);
                if (existingComment == null || existingComment.PostId != postId)
                {
                    return NotFound(ApiResponse<CommentResponseDto>.ErrorResponse($"Comment with ID {commentId} not found in post {postId}", 404));
                }

                if (!await _commentService.IsCommentOwnerAsync(commentId, Guid.Parse(currentUserId)))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<CommentResponseDto>.ErrorResponse("You can only update your own comments", 403));
                }

                var updatedComment = await _commentService.UpdateCommentAsync(commentId, updateCommentDto);
                if (updatedComment == null)
                    return NotFound(ApiResponse<CommentResponseDto>.ErrorResponse("Failed to update comment", 404));

                return Ok(ApiResponse<CommentResponseDto>.SuccessResponse(updatedComment, "Comment updated"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId} on post {PostId}", commentId, postId);
                return StatusCode(500, ApiResponse<CommentResponseDto>.ErrorResponse("Internal server error", 500));
            }
        }

        [HttpDelete("{commentId:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> DeleteComment(Guid postId, Guid commentId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized", 401));

                var existingComment = await _commentService.GetCommentByIdAsync(commentId);
                if (existingComment == null || existingComment.PostId != postId)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Comment with ID {commentId} not found in post {postId}", 404));

                if (!await _commentService.IsCommentOwnerAsync(commentId, Guid.Parse(currentUserId)))
                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.ErrorResponse("You can only delete your own comments", 403));

                var result = await _commentService.DeleteCommentAsync(commentId);
                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse("Failed to delete comment", 404));

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId} from post {PostId}", commentId, postId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Internal server error", 500));
            }
        }
    }
}
