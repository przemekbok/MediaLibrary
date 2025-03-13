using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using MediaLibrary.Models;
using Microsoft.Extensions.Configuration;
using static System.Net.Mime.MediaTypeNames;

namespace MediaLibrary.Services
{
    public class FileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly string _rootPath;
        private readonly string _mediaDir;
        private readonly string _thumbnailsDir;
        private readonly int _thumbnailWidth;
        private readonly int _thumbnailHeight;

        public FileStorageService(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;

            // Base storage path - can be configured in appsettings.json
            _rootPath = _config.GetValue<string>("MediaStorage:BasePath") ??
                        Path.Combine(_env.ContentRootPath, "MediaFiles");

            // Create subdirectories based on media type
            _mediaDir = Path.Combine(_rootPath, "Original");
            _thumbnailsDir = Path.Combine(_rootPath, "Thumbnails");

            // Thumbnail dimensions
            _thumbnailWidth = _config.GetValue<int>("MediaStorage:ThumbnailWidth", 300);
            _thumbnailHeight = _config.GetValue<int>("MediaStorage:ThumbnailHeight", 300);

            // Ensure directories exist
            EnsureDirectoriesExist();
        }

        private void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(_rootPath);
            Directory.CreateDirectory(_mediaDir);
            Directory.CreateDirectory(_thumbnailsDir);

            // Create type-specific directories
            Directory.CreateDirectory(Path.Combine(_mediaDir, "Images"));
            Directory.CreateDirectory(Path.Combine(_mediaDir, "Videos"));
            Directory.CreateDirectory(Path.Combine(_mediaDir, "Others"));
        }

        public async Task<MediaFileInfo> SaveMediaFileAsync(IFormFile file, string fileType)
        {
            // Generate a unique filename with date prefix for better organization
            var datePart = DateTime.Now.ToString("yyyyMMdd");
            var uniqueId = Guid.NewGuid().ToString("N");
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{datePart}_{uniqueId}{fileExtension}";

            // Determine directory based on file type
            var typeDir = fileType.ToLower() switch
            {
                "image" => "Images",
                "video" => "Videos",
                _ => "Others"
            };

            var filePath = Path.Combine(_mediaDir, typeDir, fileName);
            var relativeFilePath = Path.Combine("MediaFiles", "Original", typeDir, fileName)
                                      .Replace("\\", "/");

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var result = new MediaFileInfo
            {
                FileName = fileName,
                FilePath = filePath,
                RelativeFilePath = relativeFilePath,
                FileSize = file.Length
            };

            // Generate thumbnail for images
            if (fileType.ToLower() == "image")
            {
                var thumbnailInfo = await CreateImageThumbnailAsync(filePath, fileName);
                if (thumbnailInfo != null)
                {
                    result.ThumbnailPath = thumbnailInfo.FilePath;
                    result.RelativeThumbnailPath = thumbnailInfo.RelativeFilePath;
                    result.Width = thumbnailInfo.Width;
                    result.Height = thumbnailInfo.Height;
                }
            }

            // Extract metadata for videos
            else if (fileType.ToLower() == "video")
            {
                // In a production app, you might want to use a library like MediaToolkit
                // or FFmpeg to extract video metadata (dimensions, duration, etc.)
                // For simplicity, we're not implementing that here
            }

            return result;
        }

        private async Task<MediaFileInfo> CreateImageThumbnailAsync(string originalPath, string fileName)
        {
            try
            {
                var thumbnailFileName = fileName;
                var thumbnailPath = Path.Combine(_thumbnailsDir, thumbnailFileName);
                var relativeThumbnailPath = Path.Combine("MediaFiles", "Thumbnails", thumbnailFileName)
                                              .Replace("\\", "/");

                // Use ImageSharp to create and save thumbnail
                using (var image = await SixLabors.ImageSharp.Image.LoadAsync(originalPath))
                {
                    // Calculate dimensions while maintaining aspect ratio
                    var originalWidth = image.Width;
                    var originalHeight = image.Height;

                    double ratio = Math.Min((double)_thumbnailWidth / originalWidth,
                                           (double)_thumbnailHeight / originalHeight);

                    int newWidth = (int)(originalWidth * ratio);
                    int newHeight = (int)(originalHeight * ratio);

                    // Resize the image
                    image.Mutate(x => x.Resize(newWidth, newHeight));

                    // Save the thumbnail
                    await image.SaveAsync(thumbnailPath);

                    return new MediaFileInfo
                    {
                        FilePath = thumbnailPath,
                        RelativeFilePath = relativeThumbnailPath,
                        Width = originalWidth,
                        Height = originalHeight
                    };
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error creating thumbnail: {ex.Message}");
                return null;
            }
        }

        public bool DeleteMediaFile(string filePath, string thumbnailPath = null)
        {
            try
            {
                // Delete main file
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Delete thumbnail if exists
                if (!string.IsNullOrEmpty(thumbnailPath) && File.Exists(thumbnailPath))
                {
                    File.Delete(thumbnailPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error deleting file: {ex.Message}");
                return false;
            }
        }

        // Get absolute path from relative path
        public string GetAbsolutePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            // Convert relative path to absolute
            relativePath = relativePath.Replace("/", Path.DirectorySeparatorChar.ToString());
            var contentRootPath = _env.ContentRootPath;

            return Path.Combine(contentRootPath, relativePath);
        }
    }

    public class MediaFileInfo
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string RelativeFilePath { get; set; }
        public string ThumbnailPath { get; set; }
        public string RelativeThumbnailPath { get; set; }
        public long FileSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Duration { get; set; }
    }
}
