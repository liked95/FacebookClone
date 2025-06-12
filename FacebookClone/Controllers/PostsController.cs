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
        private readonly ILogger<PostsController> _logger;

        public PostsController(IPostService postService, ILogger<PostsController> logger)
        {
            _postService = postService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new post
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(PostResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PostResponseDto>> CreatePost([FromBody] CreatePostDto createPostDto)
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

                var userId = Guid.Parse(currentUserId);
                var post = await _postService.CreatePostAsync(createPostDto, userId);
                if (post == null)
                {
                    return BadRequest("Failed to create post!");
                }

                return StatusCode(201, post);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error occurred while creating post");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Update a post
        /// </summary>
        [HttpPut("{postId}")]
        [Authorize]
        [ProducesResponseType(typeof(PostResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PostResponseDto>> UpdatePost(Guid postId, [FromBody] UpdatePostDto updatePostDto)
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

                var userId = Guid.Parse(currentUserId);
                if (!await _postService.IsPostOwnerAsync(postId, userId))
                {
                    return Forbid("You can only update your own posts");
                }


                var updatedPost = await _postService.UpdatePostAsync(postId, updatePostDto);
                
                if (updatedPost == null)
                {
                    return NotFound("Failed to update post!");
                }

                return Ok(updatedPost);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating post");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Delete a post
        /// </summary>
        [HttpDelete("{postId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> DeletePost(Guid postId)
        {
            try
            {
                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized();
                }

                var userId = Guid.Parse(currentUserId);
                if (!await _postService.IsPostOwnerAsync(postId, userId))
                {
                    return Forbid("You can only delete your own posts");
                }


                var result = await _postService.DeletePostAsync(postId);

                if (!result)
                {
                    return NotFound("Failed to delete post!");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting post");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}
