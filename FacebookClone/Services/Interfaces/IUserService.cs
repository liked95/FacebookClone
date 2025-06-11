using FacebookClone.Models.DTOs;

namespace FacebookClone.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto?> GetUserByIdAsync(Guid id); 
        Task<UserResponseDto?> GetUserByUsernameAsync(string username);
        Task<UserResponseDto?> GetUserByEmailAsync(string username);
        Task<UserResponseDto?> CreateUserAsync(CreateUserDTOs createUserDto);
        Task<UserResponseDto?> UpdateUserAsync(Guid id, UpdateUserDTOs updateUserDto);
        Task<bool> DeleteUserAsync(Guid id);
    }
}
