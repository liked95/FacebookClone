using FacebookClone.Models.Constants;
using FacebookClone.Models.DomainModels;

namespace FacebookClone.Repositories.Interfaces
{
    public interface IMediaRepository
    {
        Task<MediaFile> CreateAsync(MediaFile mediaFile);
        Task<MediaFile?> GetByIdAsync(Guid id);
        Task<List<MediaFile>> GetByIdsAsync(List<Guid> ids);
        Task<List<MediaFile>> GetByAttachmentAsync(MediaAttachmentType attachmentType, string attachmentId);
        Task<MediaFile?> GetSingleByAttachmentAsync(MediaAttachmentType attachmentType, string attachmentId); // For avatars
        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeleteByAttachmentAsync(MediaAttachmentType attachmentType, string attachmentId);
        Task UpdateAsync(MediaFile mediaFile);
    }
}
