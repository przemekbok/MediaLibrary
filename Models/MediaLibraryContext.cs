using Microsoft.EntityFrameworkCore;

namespace MediaLibrary.Models
{
    public class MediaLibraryContext : DbContext
    {
        public MediaLibraryContext(DbContextOptions<MediaLibraryContext> options)
            : base(options)
        {
        }

        public DbSet<Media> Media { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<MediaTag> MediaTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the many-to-many relationship
            modelBuilder.Entity<MediaTag>()
                .HasKey(mt => new { mt.MediaId, mt.TagId });

            modelBuilder.Entity<MediaTag>()
                .HasOne(mt => mt.Media)
                .WithMany(m => m.MediaTags)
                .HasForeignKey(mt => mt.MediaId);

            modelBuilder.Entity<MediaTag>()
                .HasOne(mt => mt.Tag)
                .WithMany(t => t.MediaTags)
                .HasForeignKey(mt => mt.TagId);

            // Create indexes for better performance
            modelBuilder.Entity<Media>()
                .HasIndex(m => m.Name);

            modelBuilder.Entity<Media>()
                .HasIndex(m => m.FileType);

            modelBuilder.Entity<Media>()
                .HasIndex(m => m.UploadDate);

            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();
        }
    }
}