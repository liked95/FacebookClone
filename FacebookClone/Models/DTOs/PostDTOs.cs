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

        [Url]
        public string? ImageUrl { get; set; }

        [Url]
        public string? VideoUrl { get; set; }

        [Url]
        public string? FileUrl { get; set; }
    }

    public class UpdatePostDto
    {
        [StringLength(5000, MinimumLength = 3)]
        public string? Content { get; set; }

        public PrivacyType? Privacy { get; set; }

        [Url]
        public string? ImageUrl { get; set; }

        [Url]
        public string? VideoUrl { get; set; }

        [Url]
        public string? FileUrl { get; set; }
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
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string? FileUrl { get; set; }
        public int CommentsCount { get; set; }
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
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
