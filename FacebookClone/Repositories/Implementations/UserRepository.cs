using FacebookClone.Data;
using FacebookClone.Models.DomainModels;
using FacebookClone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FacebookClone.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext context;

        public UserRepository(AppDbContext appContext)
        {
            context = appContext;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await context.Users.FirstOrDefaultAsync(x => x.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User?> CreateUserAsync(User user)
        {
            try
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
                return user;
            }
            catch
            {
                return null;
            }
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            try
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();
                return user;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            try
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
                if (user == null)
                {
                    return false;
                }

                context.Users.Remove(user);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
