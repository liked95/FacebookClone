using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FacebookClone.Models.DTOs;
using FacebookClone.Services.Interfaces;

namespace FacebookClone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LikesController : ControllerBase
    {
        private readonly ILikeService _likeService;
        private readonly ILogger<LikesController> _logger;

        public LikesController(ILikeService likeService, ILogger<LikesController> logger)
        {
            _likeService = likeService;
            _logger = logger;
        }

        /// <summary>
        /// Toggle like on a post
        /// </summary>
        [HttpPost("posts/{postId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(LikeActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LikeActionResult>> TogglePostLike(Guid postId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var result = await _likeService.TogglePostLikeAsync(postId, Guid.Parse(currentUserId));
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling post like");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Get users who liked a post
        /// </summary>
        [Authorize]
        [HttpGet("posts/{postId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<PostLikeDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PostLikeDto>>> GetPostLikes(
            Guid postId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            try
            {
                var likes = await _likeService.GetPostLikesAsync(postId, pageNumber, pageSize);
                return Ok(likes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving post likes");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Toggle like on a comment
        /// </summary>
        [HttpPost("comments/{commentId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(LikeActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LikeActionResult>> ToggleCommentLike(Guid commentId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized();

                var result = await _likeService.ToggleCommentLikeAsync(commentId, Guid.Parse(currentUserId));
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling comment like");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Get users who liked a comment
        /// </summary>
        [Authorize]
        [HttpGet("comments/{commentId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<CommentLikeDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CommentLikeDto>>> GetCommentLikes(
            Guid commentId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            try
            {
                var likes = await _likeService.GetCommentLikesAsync(commentId, pageNumber, pageSize);
                return Ok(likes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving comment likes");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}
