using Microsoft.EntityFrameworkCore;
using FacebookClone.Data;
using FacebookClone.Models.DomainModels;
using FacebookClone.Models.DTOs;
using FacebookClone.Repositories.Interfaces;

namespace FacebookClone.Repositories.Implementations
{
    public class LikeRepository : ILikeRepository
    {
        private readonly AppDbContext _context;

        public LikeRepository(AppDbContext context)
        {
            _context = context;
        }

        // Post Likes
        public async Task<LikeActionResult> TogglePostLikeAsync(Guid postId, Guid userId)
        {
            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId);

            if (existingLike == null)
            {
                // Create new like
                var newLike = new PostLike
                {
                    Id = Guid.NewGuid(),
                    PostId = postId,
                    UserId = userId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.PostLikes.Add(newLike);
            }
            else
            {
                // Toggle existing like
                existingLike.IsActive = !existingLike.IsActive;
                existingLike.UpdatedAt = DateTime.UtcNow;
                _context.PostLikes.Update(existingLike);
            }

            await _context.SaveChangesAsync();

            var totalLikes = await GetPostLikesCountAsync(postId);
            var isLiked = existingLike?.IsActive ?? true;

            return new LikeActionResult
            {
                IsLiked = isLiked,
                TotalLikes = totalLikes,
                Message = isLiked ? "Post liked" : "Post unliked"
            };
        }

        public async Task<IEnumerable<PostLikeDto>> GetPostLikesAsync(Guid postId, int pageNumber, int pageSize)
        {
            return await _context.PostLikes
                .AsNoTracking()
                .Include(pl => pl.User)
                .Where(pl => pl.PostId == postId && pl.IsActive)
                .OrderByDescending(pl => pl.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(pl => new PostLikeDto
                {
                    Id = pl.Id,
                    UserId = pl.UserId,
                    Username = pl.User.Username,
                    UserAvatarUrl = pl.User.AvatarUrl,
                    CreatedAt = pl.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<int> GetPostLikesCountAsync(Guid postId)
        {
            return await _context.PostLikes
                .CountAsync(pl => pl.PostId == postId && pl.IsActive);
        }

        public async Task<bool> IsPostLikedByUserAsync(Guid postId, Guid userId)
        {
            return await _context.PostLikes
                .AnyAsync(pl => pl.PostId == postId && pl.UserId == userId && pl.IsActive);
        }

        // Comment Likes
        public async Task<LikeActionResult> ToggleCommentLikeAsync(Guid commentId, Guid userId)
        {
            var existingLike = await _context.CommentLikes
                .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId);

            if (existingLike == null)
            {
                // Create new like
                var newLike = new CommentLike
                {
                    Id = Guid.NewGuid(),
                    CommentId = commentId,
                    UserId = userId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.CommentLikes.Add(newLike);
            }
            else
            {
                // Toggle existing like
                existingLike.IsActive = !existingLike.IsActive;
                existingLike.UpdatedAt = DateTime.UtcNow;
                _context.CommentLikes.Update(existingLike);
            }

            await _context.SaveChangesAsync();

            var totalLikes = await GetCommentLikesCountAsync(commentId);
            var isLiked = existingLike?.IsActive ?? true;

            return new LikeActionResult
            {
                IsLiked = isLiked,
                TotalLikes = totalLikes,
                Message = isLiked ? "Comment liked" : "Comment unliked"
            };
        }

        public async Task<IEnumerable<CommentLikeDto>> GetCommentLikesAsync(Guid commentId, int pageNumber, int pageSize)
        {
            return await _context.CommentLikes
                .AsNoTracking()
                .Include(cl => cl.User)
                .Where(cl => cl.CommentId == commentId && cl.IsActive)
                .OrderByDescending(cl => cl.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(cl => new CommentLikeDto
                {
                    Id = cl.Id,
                    UserId = cl.UserId,
                    Username = cl.User.Username,
                    UserAvatarUrl = cl.User.AvatarUrl,
                    CreatedAt = cl.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<int> GetCommentLikesCountAsync(Guid commentId)
        {
            return await _context.CommentLikes
                .CountAsync(cl => cl.CommentId == commentId && cl.IsActive);
        }

        public async Task<bool> IsCommentLikedByUserAsync(Guid commentId, Guid userId)
        {
            return await _context.CommentLikes
                .AnyAsync(cl => cl.CommentId == commentId && cl.UserId == userId && cl.IsActive);
        }
    }
}
