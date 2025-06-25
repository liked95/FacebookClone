using FacebookClone.Models.Constants;
using FacebookClone.Models.DTOs;

namespace FacebookClone.Services.Interfaces
{
    public interface IMediaService
    {
        Task<List<MediaFileDto>> UploadMultipleFilesAsync(
            Guid userId,
            List<IFormFile> files,
            MediaAttachmentType attachmentType,
            string attachmentId,
            CancellationToken cancellationToken = default);
        Task<List<MediaFileDto>> GetMediaFilesByAttachmentAsync(MediaAttachmentType attachmentType, string attachmentId);
        Task<bool> DeleteMediaFilesByAttachmentAsync(MediaAttachmentType attachmentType, string attachmentId);
        Task<bool> ValidateMediaOwnershipAsync(List<Guid> mediaIds, Guid userId);
    }

}
