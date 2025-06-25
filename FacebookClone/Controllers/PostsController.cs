using FacebookClone.Common;
using FacebookClone.Models.Constants;
using FacebookClone.Models.DTOs;
using FacebookClone.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FacebookClone.Controllers
{
    [Route("Api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly ILogger<PostsController> _logger;

        public PostsController(
            IPostService postService,
            ICommentService commentService,
            ILogger<PostsController> logger)
        {
            _postService = postService;
            _commentService = commentService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new post
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PostResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<PostResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PostResponseDto>>> CreatePost(
            [FromForm] string content,
            [FromForm] PrivacyType privacy,
            [FromForm] List<IFormFile>? mediaFiles = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<PostResponseDto>.ErrorResponse("Validation failed", 400, errors));
                }

                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(ApiResponse<PostResponseDto>.ErrorResponse("Unauthorized", 401));
                }

                var userId = Guid.Parse(currentUserId);

                var createPostDto = new CreatePostDto
                {
                    Content = content?.Trim() ?? string.Empty,
                    Privacy = privacy 
                };

                var post = await _postService.CreatePostWithMediaAsync(userId, createPostDto, mediaFiles);
                if (post == null)
                {
                    return BadRequest(ApiResponse<PostResponseDto>.ErrorResponse("Failed to create post"));
                }

                return StatusCode(201, ApiResponse<PostResponseDto>.SuccessResponse(post, "Post created", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating post");
                return StatusCode(500, ApiResponse<PostResponseDto>.ErrorResponse("Internal server error", 500));
            }
        }

        /// <summary>
        /// Update a post
        /// </summary>
        [HttpPut("{postId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PostResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PostResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PostResponseDto>>> UpdatePost(Guid postId, [FromBody] UpdatePostDto updatePostDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<PostResponseDto>.ErrorResponse("Validation failed", 400, errors));
                }

                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(ApiResponse<PostResponseDto>.ErrorResponse("Unauthorized", 401));
                }

                var userId = Guid.Parse(currentUserId);
                if (!await _postService.IsPostOwnerAsync(postId, userId))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<PostResponseDto>.ErrorResponse("You can only update your own posts", 403));
                }

                var updatedPost = await _postService.UpdatePostAsync(postId, updatePostDto);
                if (updatedPost == null)
                {
                    return NotFound(ApiResponse<PostResponseDto>.ErrorResponse("Failed to update post", 404));
                }

                return Ok(ApiResponse<PostResponseDto>.SuccessResponse(updatedPost, "Post updated"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating post");
                return StatusCode(500, ApiResponse<PostResponseDto>.ErrorResponse("Internal server error", 500));
            }
        }

        /// <summary>
        /// Delete a post
        /// </summary>
        [HttpDelete("{postId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeletePost(Guid postId)
        {
            try
            {
                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized", 401));
                }

                var userId = Guid.Parse(currentUserId);
                if (!await _postService.IsPostOwnerAsync(postId, userId))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.ErrorResponse("You can only delete your own posts", 403));
                }

                var result = await _postService.DeletePostAsync(postId);
                if (!result)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Failed to delete post", 404));
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting post");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Internal server error", 500));
            }
        }
    }
}
