using System;
using System.ComponentModel.DataAnnotations;
using FacebookClone.Models.Constants;

namespace FacebookClone.Models.DomainModels
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Url]
        public string? AvatarUrl { get; set; }

        public string Role { get; set; } = RoleTypes.User;
        public DateTime CreatedAt{ get; set; } = DateTime.UtcNow;

        // Navigations
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
        public virtual ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();

    }
}
