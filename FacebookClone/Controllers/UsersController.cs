using FacebookClone.Common;
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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            IPostService postService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _postService = postService;
            _logger = logger;
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetUser(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(ApiResponse<UserResponseDto>.ErrorResponse($"User with ID {id} not found", 404));

                return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "User fetched"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user {UserId}", id);
                return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResponse("Internal server error", 500));
            }
        }

        /// <summary>
        /// Get all user's posts
        /// </summary>
        [HttpGet("{userId:guid}/Posts")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PostResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PostResponseDto>>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PostResponseDto>>>> GetUserPosts(Guid userId, [FromQuery] int pageNumber = 1, int pageSize = 25)
        {
            try
            {
                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Current user ID: {CurrentUserId}, Requested user ID: {RequestedUserId}", currentUserId, userId);

                if (currentUserId != userId.ToString())
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ApiResponse<IEnumerable<PostResponseDto>>.ErrorResponse("You can query your posts only", 403));
                }

                var posts = await _postService.GetPostsByUserIdAsync(userId, pageNumber, pageSize);
                return Ok(ApiResponse<IEnumerable<PostResponseDto>>.SuccessResponse(posts, "User posts fetched"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving posts for user {UserId}", userId);
                return StatusCode(500, ApiResponse<IEnumerable<PostResponseDto>>.ErrorResponse("Internal server error", 500));
            }
        }
    }
}
