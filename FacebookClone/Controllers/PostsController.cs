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
    }
}
