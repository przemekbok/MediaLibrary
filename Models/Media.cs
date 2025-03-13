using MediaLibrary.Models;
using System.ComponentModel.DataAnnotations;

public class Media
{
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    public string Description { get; set; }

    [Required]
    [StringLength(50)]
    public string FileType { get; set; } // "image", "video", or "other"

    [Required]
    [StringLength(10)]
    public string FileExtension { get; set; } // e.g., "jpg", "mp4"

    [Required]
    [StringLength(100)]
    public string ContentType { get; set; } // MIME type

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } // Relative path to the file

    [StringLength(500)]
    public string ThumbnailPath { get; set; } // Relative path to the thumbnail

    [Required]
    public long FileSize { get; set; } // Size in bytes

    public int? Width { get; set; } // For images and videos

    public int? Height { get; set; } // For images and videos

    public int? Duration { get; set; } // For videos (in seconds)

    [Required]
    public DateTime UploadDate { get; set; }

    [Required]
    public DateTime LastModified { get; set; }

    // Navigation property for tags
    public virtual ICollection<MediaTag> MediaTags { get; set; } = new List<MediaTag>();
}