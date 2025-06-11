using FacebookClone.Models.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacebookClone.Models.DomainModels
{
    public class Post
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(5000, MinimumLength = 3)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public PrivacyType Privacy { get; set; } = PrivacyType.Public;

        public bool IsEdited { get; set; } = false;

        // Optional URL for an image
        [Url]
        public string? ImageUrl { get; set; }

        // Optional URL for a video.
        [Url]
        public string? VideoUrl { get; set; }

        // Optional URL for a file attachment.
        [Url]
        public string? FileUrl { get; set; }

        // Navigation Properties
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    }
}
