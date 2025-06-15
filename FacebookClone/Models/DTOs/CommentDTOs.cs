using System.ComponentModel.DataAnnotations;

namespace FacebookClone.Models.DTOs
{
    public class CreateCommentDto
    {
        [Required]
        [StringLength(2000, MinimumLength = 3)]
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateCommentDto
    {
        [StringLength(2000, MinimumLength = 3)]
        public string? Content { get; set; }
    }

    public class CommentResponseDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsEdited { get; set; }
        public Guid? UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public Guid PostId { get; set; }
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
    }
}
