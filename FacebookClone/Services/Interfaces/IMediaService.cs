using FacebookClone.Models.DTOs;

namespace FacebookClone.Services.Interfaces
{
    public interface IMediaService
    {
        Task<List<MediaFileDto>> UploadMultipleFilesAsync(
            Guid userId,
            List<IFormFile> files,
            string attachmentType,
            string attachmentId,
            CancellationToken cancellationToken = default);
        Task<List<MediaFileDto>> GetMediaFilesByAttachmentAsync(string attachmentType, string attachmentId);
        Task<bool> DeleteMediaFilesByAttachmentAsync(string attachmentType, string attachmentId);
        Task<bool> ValidateMediaOwnershipAsync(List<Guid> mediaIds, Guid userId);
    }

}
