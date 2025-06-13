using FacebookClone.Models.DomainModels;
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

        public CommentsController(
            ICommentService commentService,
            ILogger<CommentsController> logger
        )
        {
            _commentService = commentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all comments for a specific post
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CommentResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetPostComments(
            Guid postId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25
        )
        {
            // TODO: Add some kind of authorization for friendsonly or public posts
            try
            {
                var comments = await _commentService.GetCommentsByPostIdAsync(postId, pageNumber, pageSize);
                //Console.WriteLine("DO go here");
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving comments for post {PostId}", postId);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Get a specific comment from a post
        /// </summary>
        [HttpGet("{commentId:guid}")]
        [ProducesResponseType(typeof(CommentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentResponseDto>> GetComment(Guid postId, Guid commentId)
        {
            try
            {
                var comment = await _commentService.GetCommentByIdAsync(commentId);
                if (comment == null || comment.PostId != postId)
                    return NotFound($"Comment with ID {commentId} not found in post {postId}");

                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving comment {CommentId} from post {PostId}", commentId, postId);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Create a new comment
        /// </summary>
        [HttpPost]
        //[Authorize]
        [ProducesResponseType(typeof(CommentResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommentResponseDto>> CreateComment(Guid postId, [FromBody] CreateCommentDto createCommentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized();
                }


                var createdComment = await _commentService.CreateCommentAsync(Guid.Parse(currentUserId), postId, createCommentDto);
                if (createdComment == null)
                {
                    return BadRequest("Failed to create comment");
                }

                return CreatedAtAction(nameof(GetComment), new { postId = postId, commentId = createdComment.Id }, createdComment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating comment on post {PostId}", postId);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Update a comment
        /// </summary>
        [HttpPut("{commentId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(CommentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CommentResponseDto>> UpdateComment(Guid postId, Guid commentId, [FromBody] UpdateCommentDto updateCommentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized();
                }

                var existingComment = await _commentService.GetCommentByIdAsync(commentId);
                if (existingComment == null || existingComment.PostId != postId)
                {
                    return NotFound($"Comment with ID {commentId} not found in post {postId}");
                }

                if (!await _commentService.IsCommentOwnerAsync(commentId, Guid.Parse(currentUserId)))
                {
                    return Forbid("You can only update your own comments");
                }

                var updatedComment = await _commentService.UpdateCommentAsync(commentId, updateCommentDto);
                if (updatedComment == null)
                    return NotFound($"Comment with ID {commentId} not found");

                return Ok(updatedComment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating comment {CommentId} on post {PostId}", commentId, postId);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Delete a comment from a post (owner only)
        /// </summary>
        [HttpDelete("{commentId:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> DeleteComment(Guid postId, Guid commentId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                // Verify comment belongs to the post
                var existingComment = await _commentService.GetCommentByIdAsync(commentId);
                if (existingComment == null || existingComment.PostId != postId)
                    return NotFound($"Comment with ID {commentId} not found in post {postId}");

                if (!await _commentService.IsCommentOwnerAsync(commentId, Guid.Parse(currentUserId)))
                    return Forbid("You can only delete your own comments");

                var result = await _commentService.DeleteCommentAsync(commentId);
                if (!result)
                    return NotFound($"Comment with ID {commentId} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting comment {CommentId} from post {PostId}", commentId, postId);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

    }
}
