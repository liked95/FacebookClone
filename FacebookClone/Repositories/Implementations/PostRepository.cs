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
        
        public Task<Post?> CreatePostAsync(Post post)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeletePostAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetAllPostsByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public  async Task<Post?> GetByIdAsync(Guid id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            return post;
        }

        public Task<Post?> UpdatePostAsync(Post post)
        {
            throw new NotImplementedException();
        }
    }
}
