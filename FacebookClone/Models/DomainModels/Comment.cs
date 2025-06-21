using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacebookClone.Models.DomainModels
{
    public class Comment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 3)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // The date and time the comment was last updated.
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsEdited { get; set; } = false;


        // Foreign to User
        public Guid? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }


        // Foreign to Post
        public Guid PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;

        public int Level { get; set; } = 0;

        // Parent comment
        public Guid? ParentCommentId { get; set; }

        [ForeignKey("ParentCommentId")]
        public virtual Comment? ParentComment { get; set; }


        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public virtual ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
    }
}
