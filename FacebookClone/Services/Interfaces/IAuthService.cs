using FacebookClone.Models.DTOs;

namespace FacebookClone.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        string GenerateJwtToken(UserResponseDto userDto);
        Task<UserResponseDto?> GetCurrentUserAsync(Guid userId);
    }
}
