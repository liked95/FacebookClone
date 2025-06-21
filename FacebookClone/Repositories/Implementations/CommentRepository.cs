using FacebookClone.Data;
using FacebookClone.Models.DomainModels;
using FacebookClone.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace FacebookClone.Repositories.Implementations
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;

        public CommentRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Comment?> CreateCommentAsync(Comment comment)
        {
            try
            {
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                return comment;
            }
            catch
            {
                return null;
            }
        }


        public async Task<Comment?> UpdateCommentAsync(Comment comment)
        {
            try
            {
                comment.IsEdited = true;
                comment.UpdatedAt = DateTime.UtcNow;
                _context.Comments.Update(comment);
                await _context.SaveChangesAsync();
                return await GetCommentByIdAsync(comment.Id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteCommentAsync(Guid id)
        {
            try
            {
                var comment = await GetCommentByIdAsync(id);
                if (comment == null)
                {
                    return false;
                }

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Comment?> GetCommentByIdAsync(Guid id)
        {
            return await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId, int pageNumber, int pageSize, Guid? parentCommentId)
        {
            var query = _context.Comments
                .AsNoTracking()
                .Include(c => c.User)
                .Where(c => c.PostId == postId);



            if (parentCommentId.HasValue)
            {
                query = query.Where(c => c.ParentCommentId == parentCommentId);
            }
            else
            {
                query = query.Where(c => c.ParentCommentId == null);
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> IsCommentOwnerAsync(Guid commentId, Guid ownerId)
        {
            return await _context.Comments.AnyAsync(c => c.Id == commentId && c.UserId == ownerId);
        }


        public async Task<int> GetPostCommentsCountAsync(Guid postId)
        {
            return await _context.Comments.CountAsync(c => c.PostId == postId);
        }

        public async Task<int> GetReplyCountByCommentIdAsync(Guid id)
        {
            return await _context.Comments.CountAsync(c => c.ParentCommentId == id);
        }

        public async Task<IEnumerable<Comment>> GetRepliesForCommentAsync(Guid parentCommentId, int pageNumber, int pageSize)
        {
            return await _context.Comments
                .AsNoTracking()
                .Include(c=>c.User)
                .Where(c => c.ParentCommentId == parentCommentId)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
