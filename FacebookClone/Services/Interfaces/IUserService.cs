using FacebookClone.Models.DTOs;

namespace FacebookClone.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto?> GetUserByIdAsync(Guid id); 
    }
}
