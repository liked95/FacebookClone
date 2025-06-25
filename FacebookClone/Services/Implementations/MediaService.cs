using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FacebookClone.Models.Constants;
using FacebookClone.Models.DomainModels;
using FacebookClone.Models.DTOs;
using FacebookClone.Repositories.Interfaces;
using FacebookClone.Services.Interfaces;


namespace FacebookClone.Services.Implementations
{
    public class MediaService : IMediaService
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<MediaService> _logger;
        private readonly string _containerName;

        public MediaService(
            IMediaRepository mediaRepository,
            BlobServiceClient blobServiceClient,
            IConfiguration configuration,
            ILogger<MediaService> logger)
        {
            _mediaRepository = mediaRepository;
            _blobServiceClient = blobServiceClient;
            _logger = logger;
            _containerName = configuration["AzureStorage:ContainerName"] ?? "media-files";
        }

        public async Task<List<MediaFileDto>> UploadMultipleFilesAsync(
            Guid userId,
            List<IFormFile> files,
            MediaAttachmentType attachmentType,
            string attachmentId,
            CancellationToken cancellationToken = default)
        {
            var uploadResults = new List<MediaFileDto>();
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            // Ensure container exists
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];

                try
                {
                    // Validate file
                    ValidateFile(file);

                    // Generate unique filename with folder structure
                    var fileExtension = Path.GetExtension(file.FileName);
                    var fileName = $"{attachmentType.ToString()}/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}{fileExtension}";
                    var blobClient = containerClient.GetBlobClient(fileName);

                    // Set blob metadata
                    var metadata = new Dictionary<string, string>
                    {
                        ["OriginalFileName"] = file.FileName,
                        ["UploadedBy"] = userId.ToString(),
                        ["AttachmentType"] = attachmentType.ToString(),
                        ["AttachmentId"] = attachmentId,
                        ["UploadedAt"] = DateTime.UtcNow.ToString("O")
                    };

                    // Upload to Azure Blob with metadata
                    using var stream = file.OpenReadStream();
                    var blobUploadOptions = new BlobUploadOptions
                    {
                        Metadata = metadata,
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = file.ContentType
                        }
                    };

                    await blobClient.UploadAsync(stream, blobUploadOptions, cancellationToken);

                    // Create media file entity
                    var mediaFile = new MediaFile
                    {
                        Id = Guid.NewGuid(),
                        FileName = fileName,
                        OriginalFileName = file.FileName,
                        FileSize = file.Length,
                        MimeType = file.ContentType,
                        MediaType = file.ContentType.StartsWith("image/") ? "image" : "video",
                        BlobUrl = blobClient.Uri.ToString(),
                        UploadedBy = userId,
                        AttachmentType = attachmentType,
                        AttachmentId = attachmentId,
                        DisplayOrder = i + 1,
                        ProcessingStatus = "completed",
                        IsProcessed = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // Save to database
                    var savedMediaFile = await _mediaRepository.CreateAsync(mediaFile);
                    uploadResults.Add(MapToDto(savedMediaFile));

                    _logger.LogInformation("Successfully uploaded media file {FileName} for user {UserId}",
                        file.FileName, userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload file {FileName} for user {UserId}",
                        file.FileName, userId);
                    throw;
                }
            }

            return uploadResults;
        }

        public async Task<List<MediaFileDto>> GetMediaFilesByAttachmentAsync(MediaAttachmentType attachmentType, string attachmentId)
        {
            var mediaFiles = await _mediaRepository.GetByAttachmentAsync(attachmentType, attachmentId);
            return mediaFiles.Select(MapToDto).ToList();
        }

        public async Task<bool> DeleteMediaFilesByAttachmentAsync(MediaAttachmentType attachmentType, string attachmentId)
        {
            try
            {
                var mediaFiles = await _mediaRepository.GetByAttachmentAsync(attachmentType, attachmentId);
                if (!mediaFiles.Any()) return true;

                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

                // Delete from Azure Blob Storage
                foreach (var mediaFile in mediaFiles)
                {
                    try
                    {
                        var blobClient = containerClient.GetBlobClient(mediaFile.FileName);
                        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

                        _logger.LogInformation("Deleted blob {FileName} from Azure Storage", mediaFile.FileName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to delete blob {FileName} from Azure Storage", mediaFile.FileName);
                    }
                }

                // Delete from database
                await _mediaRepository.DeleteByAttachmentAsync(attachmentType, attachmentId);

                _logger.LogInformation("Deleted {Count} media files for {AttachmentType} {AttachmentId}",
                    mediaFiles.Count, attachmentType, attachmentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting media files for {AttachmentType} {AttachmentId}",
                    attachmentType, attachmentId);
                return false;
            }
        }

        public async Task<bool> ValidateMediaOwnershipAsync(List<Guid> mediaIds, Guid userId)
        {
            var mediaFiles = await _mediaRepository.GetByIdsAsync(mediaIds);
            return mediaFiles.All(m => m.UploadedBy == userId);
        }

        private void ValidateFile(IFormFile file)
        {
            var allowedImageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "image/bmp" };
            var allowedVideoTypes = new[] { "video/mp4", "video/mov", "video/avi" };

            var isValidType = allowedImageTypes.Contains(file.ContentType) ||
                             allowedVideoTypes.Contains(file.ContentType);

            if (!isValidType)
            {
                throw new ArgumentException($"File type {file.ContentType} is not supported");
            }

            var maxImageSize = 10 * 1024 * 1024; // 10MB
            var maxVideoSize = 100 * 1024 * 1024; // 100MB

            var maxSize = file.ContentType.StartsWith("image/") ? maxImageSize : maxVideoSize;

            if (file.Length > maxSize)
            {
                var maxSizeMB = maxSize / (1024 * 1024);
                throw new ArgumentException($"File size exceeds the maximum limit of {maxSizeMB}MB");
            }

            if (file.Length == 0)
            {
                throw new ArgumentException("File is empty");
            }
        }

        private MediaFileDto MapToDto(MediaFile mediaFile)
        {
            return new MediaFileDto
            {
                Id = mediaFile.Id,
                FileName = mediaFile.FileName,
                OriginalFileName = mediaFile.OriginalFileName,
                FileSize = mediaFile.FileSize,
                MimeType = mediaFile.MimeType,
                MediaType = mediaFile.MediaType,
                BlobUrl = mediaFile.BlobUrl,
                ThumbnailUrl = mediaFile.ThumbnailUrl,
                Width = mediaFile.Width,
                Height = mediaFile.Height,
                Duration = mediaFile.Duration,
                DisplayOrder = mediaFile.DisplayOrder,
                ProcessingStatus = mediaFile.ProcessingStatus,
                IsProcessed = mediaFile.IsProcessed,
                CreatedAt = mediaFile.CreatedAt
            };
        }
    }

}
