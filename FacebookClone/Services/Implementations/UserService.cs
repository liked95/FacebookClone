using FacebookClone.Models.Constants;
using FacebookClone.Models.DomainModels;
using FacebookClone.Models.DTOs;
using FacebookClone.Repositories.Interfaces;
using FacebookClone.Services.Interfaces;

namespace FacebookClone.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<UserResponseDto?> CreateUserAsync(CreateUserDTOs createUserDto)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                AvatarUrl = createUserDto.AvatarUrl,
                Role = string.IsNullOrEmpty(createUserDto.Role) ? RoleTypes.User : createUserDto.Role,
                CreatedAt = DateTime.Now,
            };

            var createdUser = await _userRepository.CreateUserAsync(user);
            if (createdUser == null)
            {
                _logger.LogError("Failed to create user with username: {Username}", createUserDto.Username);
                return null;
            }

            _logger.LogInformation("User created successfully with ID: {UserId}", createdUser.Id);
            return MapToUserResponseDto(createdUser);
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            return await _userRepository.DeleteUserAsync(id);
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : MapToUserResponseDto(user);
        }

        public async Task<UserResponseDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return user == null ? null : MapToUserResponseDto(user);
        }

        public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user == null ? null : MapToUserResponseDto(user);
        }

        public async Task<UserResponseDto?> UpdateUserAsync(Guid id, UpdateUserDTOs updateUserDto)
        {
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null)
                return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateUserDto.Username))
                existingUser.Username = updateUserDto.Username;

            if (!string.IsNullOrEmpty(updateUserDto.Email))
                existingUser.Email = updateUserDto.Email;

            if (updateUserDto.AvatarUrl != null)
                existingUser.AvatarUrl = updateUserDto.AvatarUrl;

            var updatedUser = await _userRepository.UpdateUserAsync(existingUser);
            if (updatedUser == null)
            {
                _logger.LogError("Failed to update user with ID: {UserId}", id);
                return null;
            }

            _logger.LogInformation("User updated successfully with ID: {UserId}", updatedUser.Id);
            return MapToUserResponseDto(updatedUser);
        }

        // Helper methods
        public static UserResponseDto MapToUserResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
            };
        }
    }
}
