﻿using Microsoft.EntityFrameworkCore;
using FacebookClone.Models.DomainModels;

namespace FacebookClone.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<MediaFile> MediaFiles { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique(); // Ensure email is unique

            // Configure cascade delete behaviors to avoid multiple cascade paths

            // User -> Post relationship (keep cascade)
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // User -> Comment relationship (change to NO ACTION)
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Post -> Comment relationship (keep cascade)
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // PostLike configuration
            modelBuilder.Entity<PostLike>()
                .HasIndex(pl => new { pl.UserId, pl.PostId })
                .IsUnique(); // One like record per user per post

            modelBuilder.Entity<PostLike>()
                .HasIndex(pl => new { pl.PostId, pl.IsActive }); // For efficient counting

            // CommentLike configuration
            modelBuilder.Entity<CommentLike>()
                .HasIndex(cl => new { cl.UserId, cl.CommentId })
                .IsUnique(); // One like record per user per comment

            modelBuilder.Entity<CommentLike>()
                .HasIndex(cl => new { cl.CommentId, cl.IsActive }); // For efficient counting

            // Configure relationships
            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.User)
                .WithMany(u => u.PostLikes)
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(pl => pl.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommentLike>()
                .HasOne(cl => cl.User)
                .WithMany(u => u.CommentLikes)
                .HasForeignKey(cl => cl.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CommentLike>()
                .HasOne(cl => cl.Comment)
                .WithMany(c => c.Likes)
                .HasForeignKey(cl => cl.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MediaFile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.MimeType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MediaType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.BlobUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ProcessingStatus).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AttachmentType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AttachmentId).IsRequired().HasMaxLength(50);

                // Foreign key to User
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UploadedBy)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes for polymorphic queries
                entity.HasIndex(e => new { e.AttachmentType, e.AttachmentId });
                entity.HasIndex(e => e.UploadedBy);
                entity.HasIndex(e => new { e.AttachmentType, e.AttachmentId, e.DisplayOrder });
            });
        }

    }
}
