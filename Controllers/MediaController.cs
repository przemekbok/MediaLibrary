using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaLibrary.Models;
using MediaLibrary.Services;
using MediaLibrary.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace MediaLibrary.Controllers
{
    public class MediaController : Controller
    {
        private readonly MediaLibraryContext _context;
        private readonly FileStorageService _fileStorage;
        private readonly IWebHostEnvironment _env;

        public MediaController(
            MediaLibraryContext context,
            FileStorageService fileStorage,
            IWebHostEnvironment env)
        {
            _context = context;
            _fileStorage = fileStorage;
            _env = env;
        }

        // GET: Media
        public async Task<IActionResult> Index(MediaSearchViewModel searchModel = null)
        {
            if (searchModel == null)
            {
                searchModel = new MediaSearchViewModel();
            }

            var query = _context.Media.AsQueryable();

            // Apply search filters if provided
            if (!string.IsNullOrEmpty(searchModel.SearchTerm))
            {
                query = query.Where(m =>
                    m.Name.Contains(searchModel.SearchTerm) ||
                    m.Description.Contains(searchModel.SearchTerm));
            }

            if (!string.IsNullOrEmpty(searchModel.FileType))
            {
                query = query.Where(m => m.FileType == searchModel.FileType);
            }

            if (searchModel.SelectedTags != null && searchModel.SelectedTags.Any())
            {
                foreach (var tag in searchModel.SelectedTags)
                {
                    query = query.Where(m => m.MediaTags.Any(mt => mt.Tag.Name == tag));
                }
            }

            // Get all available tags for the filter UI
            ViewBag.AllTags = await _context.Tags.Select(t => t.Name).ToListAsync();

            // Execute query and map to view models
            var mediaItems = await query
                .OrderByDescending(m => m.UploadDate)
                .Select(m => new MediaViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    FileType = m.FileType,
                    ContentType = m.ContentType,
                    FileSize = m.FileSize,
                    FilePath = m.FilePath,
                    ThumbnailPath = m.ThumbnailPath,
                    Width = m.Width,
                    Height = m.Height,
                    UploadDate = m.UploadDate,
                    Tags = m.MediaTags.Select(mt => mt.Tag.Name).ToList()
                })
                .ToListAsync();

            searchModel.Results = mediaItems;
            return View(searchModel);
        }

        // GET: Media/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var media = await _context.Media
                .Include(m => m.MediaTags)
                .ThenInclude(mt => mt.Tag)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (media == null)
            {
                return NotFound();
            }

            var viewModel = new MediaViewModel
            {
                Id = media.Id,
                Name = media.Name,
                Description = media.Description,
                FileType = media.FileType,
                ContentType = media.ContentType,
                FileSize = media.FileSize,
                FilePath = media.FilePath,
                ThumbnailPath = media.ThumbnailPath,
                Width = media.Width,
                Height = media.Height,
                Duration = media.Duration,
                UploadDate = media.UploadDate,
                Tags = media.MediaTags.Select(mt => mt.Tag.Name).ToList()
            };

            return View(viewModel);
        }

        // GET: Media/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Media/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)] // 1GB
        public async Task<IActionResult> Create(MediaUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Determine file type
                    string fileType = model.MediaFile.ContentType.StartsWith("image/")
                        ? "image"
                        : (model.MediaFile.ContentType.StartsWith("video/") ? "video" : "other");

                    // Save file to disk
                    var fileInfo = await _fileStorage.SaveMediaFileAsync(model.MediaFile, fileType);

                    // Create the media entity
                    var media = new Media
                    {
                        Name = model.Name,
                        Description = model.Description,
                        FileType = fileType,
                        FileExtension = System.IO.Path.GetExtension(model.MediaFile.FileName).TrimStart('.'),
                        ContentType = model.MediaFile.ContentType,
                        FilePath = fileInfo.RelativeFilePath,
                        ThumbnailPath = fileInfo.RelativeThumbnailPath,
                        FileSize = fileInfo.FileSize,
                        Width = fileInfo.Width,
                        Height = fileInfo.Height,
                        Duration = fileInfo.Duration,
                        UploadDate = DateTime.Now,
                        LastModified = DateTime.Now
                    };

                    // Process tags
                    if (!string.IsNullOrEmpty(model.Tags))
                    {
                        var tagNames = model.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .Distinct();

                        foreach (var tagName in tagNames)
                        {
                            // Find existing tag or create new one
                            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                            if (tag == null)
                            {
                                tag = new Tag { Name = tagName };
                                _context.Tags.Add(tag);
                                await _context.SaveChangesAsync(); // Save to get the tag ID
                            }

                            // Add the relationship
                            media.MediaTags = media.MediaTags ?? new List<MediaTag>();
                            media.MediaTags.Add(new MediaTag { TagId = tag.Id });
                        }
                    }

                    _context.Add(media);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Media/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var media = await _context.Media
                .Include(m => m.MediaTags)
                .ThenInclude(mt => mt.Tag)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (media == null)
            {
                return NotFound();
            }

            var viewModel = new MediaEditViewModel
            {
                Id = media.Id,
                Name = media.Name,
                Description = media.Description,
                Tags = string.Join(", ", media.MediaTags.Select(mt => mt.Tag.Name)),
                FilePath = media.FilePath,
                ThumbnailPath = media.ThumbnailPath,
                FileType = media.FileType
            };

            return View(viewModel);
        }

        // POST: Media/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MediaEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var media = await _context.Media
                        .Include(m => m.MediaTags)
                        .FirstOrDefaultAsync(m => m.Id == id);

                    if (media == null)
                    {
                        return NotFound();
                    }

                    // Update basic properties
                    media.Name = model.Name;
                    media.Description = model.Description;
                    media.LastModified = DateTime.Now;

                    // Update tags
                    // First, remove all existing tags
                    _context.MediaTags.RemoveRange(media.MediaTags);

                    // Then add the new tags
                    if (!string.IsNullOrEmpty(model.Tags))
                    {
                        var tagNames = model.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .Distinct();

                        foreach (var tagName in tagNames)
                        {
                            // Find existing tag or create new one
                            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                            if (tag == null)
                            {
                                tag = new Tag { Name = tagName };
                                _context.Tags.Add(tag);
                                await _context.SaveChangesAsync(); // Save to get the tag ID
                            }

                            // Add the relationship
                            media.MediaTags = media.MediaTags ?? new List<MediaTag>();
                            media.MediaTags.Add(new MediaTag { MediaId = media.Id, TagId = tag.Id });
                        }
                    }

                    _context.Update(media);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MediaExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Media/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var media = await _context.Media
                .Include(m => m.MediaTags)
                .ThenInclude(mt => mt.Tag)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (media == null)
            {
                return NotFound();
            }

            var viewModel = new MediaViewModel
            {
                Id = media.Id,
                Name = media.Name,
                Description = media.Description,
                FileType = media.FileType,
                ContentType = media.ContentType,
                FileSize = media.FileSize,
                FilePath = media.FilePath,
                UploadDate = media.UploadDate,
                Tags = media.MediaTags.Select(mt => mt.Tag.Name).ToList()
            };

            return View(viewModel);
        }

        // POST: Media/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var media = await _context.Media.FindAsync(id);

            if (media != null)
            {
                // Get absolute file paths
                string filePath = _fileStorage.GetAbsolutePath(media.FilePath);
                string thumbnailPath = _fileStorage.GetAbsolutePath(media.ThumbnailPath);

                // Delete the physical files
                _fileStorage.DeleteMediaFile(filePath, thumbnailPath);

                // Remove from database
                _context.Media.Remove(media);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Media/File/5
        public async Task<IActionResult> File(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var media = await _context.Media.FindAsync(id);

            if (media == null)
            {
                return NotFound();
            }

            // Get absolute file path
            string filePath = _fileStorage.GetAbsolutePath(media.FilePath);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            return PhysicalFile(filePath, media.ContentType);
        }

        // GET: Media/Thumbnail/5
        public async Task<IActionResult> Thumbnail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var media = await _context.Media.FindAsync(id);

            if (media == null || string.IsNullOrEmpty(media.ThumbnailPath))
            {
                return NotFound();
            }

            // Get absolute thumbnail path
            string thumbnailPath = _fileStorage.GetAbsolutePath(media.ThumbnailPath);

            if (!System.IO.File.Exists(thumbnailPath))
            {
                return NotFound();
            }

            return PhysicalFile(thumbnailPath, media.ContentType);
        }

        private bool MediaExists(int id)
        {
            return _context.Media.Any(e => e.Id == id);
        }
    }
}