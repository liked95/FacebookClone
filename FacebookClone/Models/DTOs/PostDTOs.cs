using System.ComponentModel.DataAnnotations;
using FacebookClone.Models.Constants;

namespace FacebookClone.Models.DTOs
{
    public class CreatePostDto
    {
        [Required]
        [StringLength(5000, MinimumLength = 3)]
        public string Content { get; set; } = string.Empty;

        public PrivacyType Privacy { get; set; } = PrivacyType.Public;
    }

    public class UpdatePostDto
    {
        [StringLength(5000, MinimumLength = 3)]
        public string? Content { get; set; }

        public PrivacyType? Privacy { get; set; }
        public List<Guid>? ExistingMediaIds { get; set; }
    }

    public class PostResponseDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid? UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public PrivacyType Privacy { get; set; }
        public bool IsEdited { get; set; }
        public int CommentsCount { get; set; }
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public List<MediaFileDto> MediaFiles { get; set; } = new List<MediaFileDto>();
    }

    public class MediaFileDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public long FileSize { get; set; }
        public string MimeType { get; set; }
        public string MediaType { get; set; }
        public string BlobUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Duration { get; set; }
        public int DisplayOrder { get; set; }
        public string ProcessingStatus { get; set; }
        public bool IsProcessed { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PostListDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public PrivacyType Privacy { get; set; }
        public bool IsEdited { get; set; }
        public string? ImageUrl { get; set; }
        public int CommentsCount { get; set; }
    }
}
