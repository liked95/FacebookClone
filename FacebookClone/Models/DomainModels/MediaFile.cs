using FacebookClone.Models.Constants;

namespace FacebookClone.Models.DomainModels
{
    public class MediaFile
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public long FileSize { get; set; }
        public string MimeType { get; set; }
        public string MediaType { get; set; } // "image" or "video"
        public string BlobUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Duration { get; set; } // for videos in seconds
        public Guid UploadedBy { get; set; }

        // Polymorphic association fields
        public MediaAttachmentType AttachmentType { get; set; } // "post", "avatar", "chat_message"
        public string AttachmentId { get; set; } // ID of the related entity as string
        public int DisplayOrder { get; set; } // Order within the attachment (for posts with multiple media)

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsProcessed { get; set; }
        public string ProcessingStatus { get; set; }

        // Navigation properties
        public User User { get; set; }
    }

}
