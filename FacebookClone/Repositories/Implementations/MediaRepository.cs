using FacebookClone.Data;
using FacebookClone.Models.Constants;
using FacebookClone.Models.DomainModels;
using FacebookClone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FacebookClone.Repositories.Implementations
{
    public class MediaRepository : IMediaRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MediaRepository> _logger;

        public MediaRepository(AppDbContext context, ILogger<MediaRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MediaFile> CreateAsync(MediaFile mediaFile)
        {
            _context.MediaFiles.Add(mediaFile);
            await _context.SaveChangesAsync();
            return mediaFile;
        }

        public async Task<MediaFile?> GetByIdAsync(Guid id)
        {
            return await _context.MediaFiles.FindAsync(id);
        }

        public async Task<List<MediaFile>> GetByIdsAsync(List<Guid> ids)
        {
            return await _context.MediaFiles
                .Where(m => ids.Contains(m.Id))
                .ToListAsync();
        }

        public async Task<List<MediaFile>> GetByAttachmentAsync(MediaAttachmentType attachmentType, string attachmentId)
        {
            return await _context.MediaFiles
                .Where(m => m.AttachmentType == attachmentType && m.AttachmentId == attachmentId)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
        }

        public async Task<MediaFile?> GetSingleByAttachmentAsync(MediaAttachmentType attachmentType, string attachmentId)
        {
            return await _context.MediaFiles
                .Where(m => m.AttachmentType == attachmentType && m.AttachmentId == attachmentId)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var mediaFile = await _context.MediaFiles.FindAsync(id);
            if (mediaFile == null) return false;

            _context.MediaFiles.Remove(mediaFile);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByAttachmentAsync(MediaAttachmentType attachmentType, string attachmentId)
        {
            var mediaFiles = await GetByAttachmentAsync(attachmentType, attachmentId);
            if (!mediaFiles.Any()) return true;

            _context.MediaFiles.RemoveRange(mediaFiles);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateAsync(MediaFile mediaFile)
        {
            _context.MediaFiles.Update(mediaFile);
            await _context.SaveChangesAsync();
        }
    }

}
