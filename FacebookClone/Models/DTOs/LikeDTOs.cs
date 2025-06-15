namespace FacebookClone.Models.DTOs
{
    public class PostLikeDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CommentLikeDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LikeActionResult
    {
        public bool IsLiked { get; set; }
        public int TotalLikes { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
