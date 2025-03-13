using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MediaLibrary.ViewModels
{
    public class MediaViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FileType { get; set; }
        public string ContentType { get; set; }
        public string FilePath { get; set; }
        public string ThumbnailPath { get; set; }
        public long FileSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Duration { get; set; }
        public DateTime UploadDate { get; set; }
        public List<string> Tags { get; set; } = new List<string>();

        // Helper properties for views
        public string FormattedFileSize =>
            FileSize >= 1024 * 1024
                ? $"{FileSize / (1024 * 1024.0):F2} MB"
                : $"{FileSize / 1024.0:F2} KB";

        public string Dimensions =>
            Width.HasValue && Height.HasValue
                ? $"{Width} × {Height}"
                : null;

        public string FormattedDuration
        {
            get
            {
                if (!Duration.HasValue) return null;

                var timeSpan = TimeSpan.FromSeconds(Duration.Value);
                return timeSpan.Hours > 0
                    ? $"{timeSpan.Hours}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}"
                    : $"{timeSpan.Minutes}:{timeSpan.Seconds:D2}";
            }
        }
    }

    public class MediaUploadViewModel
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public IFormFile MediaFile { get; set; }

        public string Tags { get; set; } // Comma-separated tags
    }

    public class MediaEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        public string Tags { get; set; } // Comma-separated tags

        // For display only
        public string FilePath { get; set; }
        public string ThumbnailPath { get; set; }
        public string FileType { get; set; }
    }

    public class MediaSearchViewModel
    {
        public string SearchTerm { get; set; }
        public List<string> SelectedTags { get; set; } = new List<string>();
        public string FileType { get; set; } // "image", "video", or null for all
        public List<MediaViewModel> Results { get; set; } = new List<MediaViewModel>();
    }
}