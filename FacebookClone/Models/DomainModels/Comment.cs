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
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;


        // Foreign to Post
        [Required]
        public Guid PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;


        // The properties for threaded replies (ParentCommentId, Replies, Depth)
        // have been removed for simplicity, allowing only one level of comments
    }
}
