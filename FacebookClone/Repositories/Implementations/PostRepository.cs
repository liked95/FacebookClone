using FacebookClone.Data;
using FacebookClone.Models.DomainModels;
using FacebookClone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FacebookClone.Repositories.Implementations
{
    public class PostRepository : IPostRepository
    {
        private readonly AppDbContext _context;

        public PostRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<Post?> CreatePostAsync(Post post)
        {
            try
            {
                _context.Posts.Add(post);
                await _context.SaveChangesAsync();
                return await GetByIdAsync(post.Id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Post?> UpdatePostAsync(Post post)
        {
            try
            {
                post.IsEdited = true;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
                return await GetByIdAsync(post.Id); 
            } catch
            {
                return null;
            }
        }

        public async Task<bool> DeletePostAsync(Guid id)
        {
            try
            {
                var post = await GetByIdAsync(id);
                if (post == null)
                {
                    return false;
                }

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Post>> GetAllPostsByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await _context.Posts
                .AsNoTracking()
                .Include(p => p.User)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p=>p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public  async Task<Post?> GetByIdAsync(Guid id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            return post;
        }

        public async Task<bool> IsPostOwnerAsync(Guid postId, Guid userId)
        {
            return await _context.Posts.AnyAsync(p => p.Id == postId && p.UserId == userId);
        }
    }
}
