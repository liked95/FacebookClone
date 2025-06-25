using FacebookClone.Models.DomainModels;

namespace FacebookClone.Repositories.Interfaces
{
    public interface IMediaRepository
    {
        Task<MediaFile> CreateAsync(MediaFile mediaFile);
        Task<MediaFile?> GetByIdAsync(Guid id);
        Task<List<MediaFile>> GetByIdsAsync(List<Guid> ids);
        Task<List<MediaFile>> GetByAttachmentAsync(string attachmentType, string attachmentId);
        Task<MediaFile?> GetSingleByAttachmentAsync(string attachmentType, string attachmentId); // For avatars
        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeleteByAttachmentAsync(string attachmentType, string attachmentId);
        Task UpdateAsync(MediaFile mediaFile);
    }
}
