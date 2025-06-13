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
    [Route("Api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("Register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Invalid input", 400, errors));
                }

                var result = await _authService.RegisterAsync(registerDto);
                if (result == null)
                {
                    return Conflict(ApiResponse<AuthResponseDto>.ErrorResponse("Username or email already exists", 409));
                }

                return CreatedAtAction(nameof(GetCurrentUser), null, ApiResponse<AuthResponseDto>.SuccessResponse(result, "Registration successful", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user registration");
                return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResponse("Internal server error", 500));
            }
        }

        [HttpPost("Login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Invalid input", 400, errors));
                }

                var result = await _authService.LoginAsync(loginDto);
                if (result == null)
                    return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password", 401));

                return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Login successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user login");
                return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResponse("Internal server error", 500));
            }
        }

        [HttpGet("Me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetCurrentUser()
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<UserResponseDto>.ErrorResponse("Unauthorized access", 401));

                var user = await _authService.GetCurrentUserAsync(Guid.Parse(userId));
                if (user == null)
                    return Unauthorized(ApiResponse<UserResponseDto>.ErrorResponse("User not found", 401));

                return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "User fetched"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting current user");
                return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResponse("Internal server error", 500));
            }
        }
    }
}
