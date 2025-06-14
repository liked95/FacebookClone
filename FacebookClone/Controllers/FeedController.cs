using FacebookClone.Common;
using FacebookClone.Models.DTOs;
using FacebookClone.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacebookClone.Controllers
{
    [Route("Api/[controller]")]
    [ApiController]
    public class FeedController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ILogger<UsersController> _logger;
        public FeedController(IPostService postService, ILogger<UsersController> logger)
        {
            _postService = postService;
            _logger = logger;
        }

        /// <summary>
        /// Get all posts from all users for feed population
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PostResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PostResponseDto>>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PostResponseDto>>>> GetFeedPosts([FromQuery] int pageNumber = 1, int pageSize = 25)
        {
            try
            {
                var posts = await _postService.GetPostsForFeed(pageNumber, pageSize);
                return Ok(ApiResponse<IEnumerable<PostResponseDto>>.SuccessResponse(posts, "Feed posts fetched"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving feed posts");
                return StatusCode(500, ApiResponse<IEnumerable<PostResponseDto>>.ErrorResponse("Internal server error", 500));
            }
        }
    }
}
