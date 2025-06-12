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

        public UsersController(IUserService userService,
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
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponseDto>> GetUser(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound($"User with ID {id} not found");

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user {UserId}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Get all user's posts
        /// </summary>
        
        [HttpGet("{userId:guid}/Posts")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetUserPosts(Guid userId, [FromQuery] int pageNumber = 1, int pageSize = 25)
        {
            var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != userId.ToString())
            {
                return Forbid("You can query your posts only");
            }

            var posts = await _postService.GetPostsByUserIdAsync(userId, pageNumber, pageSize);
            return Ok(posts);
        }
    }
}
