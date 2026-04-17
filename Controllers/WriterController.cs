using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using story_web.Data;
using story_web.Extensions;
using story_web.Filters;
using story_web.Models;

namespace story_web.Controllers;

[Auth]
public class WriterController : Controller
{
    private readonly AppDbContext _context;

    public WriterController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(Stories));
    }

    public async Task<IActionResult> Stories()
    {
        if (!await HasRegisteredAuthorAsync())
        {
            TempData["InfoMessage"] = "Please register as an author before creating stories.";
            return RedirectToAction(nameof(RegisterAuthor));
        }

        var currentUserId = HttpContext.Session.GetCurrentUserId()!.Value;

        var stories = await _context.Stories
            .AsNoTracking()
            .Include(item => item.Author)
            .Include(item => item.Category)
            .Include(item => item.Chapters)
            .Where(item => item.Author != null && item.Author.id_User == currentUserId)
            .OrderByDescending(item => item.Modified_At ?? item.Posted_At)
            .Select(item => new WriterStoryListItemViewModel
            {
                StoryId = item.id_Story,
                StoryName = item.StoryName ?? "Untitled story",
                CategoryName = item.Category != null ? item.Category.CategoryName : null,
                PostStatus = item.PostStatus,
                ChapterCount = item.Chapters.Count,
                ModifiedAt = item.Modified_At ?? item.Posted_At
            })
            .ToListAsync();

        return View(stories);
    }

    [HttpGet]
    public async Task<IActionResult> RegisterAuthor()
    {
        var author = await GetCurrentAuthorAsync();

        var model = new AuthorRegistrationViewModel
        {
            PenName = author?.PenName ?? string.Empty,
            Bio = author?.Bio,
            Avatar = author?.Avatar
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterAuthor(AuthorRegistrationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUserId = HttpContext.Session.GetCurrentUserId()!.Value;
        var normalizedPenName = model.PenName.Trim();

        var duplicatePenNameExists = await _context.Authors
            .AsNoTracking()
            .AnyAsync(item =>
                item.id_User != currentUserId &&
                item.PenName != null &&
                item.PenName.ToLower() == normalizedPenName.ToLower());

        if (duplicatePenNameExists)
        {
            ModelState.AddModelError(nameof(model.PenName), "This pen name is already in use.");
            return View(model);
        }

        var author = await _context.Authors.FirstOrDefaultAsync(item => item.id_User == currentUserId);
        if (author is null)
        {
            author = new Author
            {
                id_User = currentUserId
            };

            _context.Authors.Add(author);
        }

        author.PenName = normalizedPenName;
        author.Bio = string.IsNullOrWhiteSpace(model.Bio) ? null : model.Bio.Trim();
        author.Avatar = string.IsNullOrWhiteSpace(model.Avatar) ? null : model.Avatar.Trim();

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Author profile saved successfully.";
        return RedirectToAction(nameof(Stories));
    }

    [HttpGet]
    public async Task<IActionResult> CreateStory()
    {
        if (!await HasRegisteredAuthorAsync())
        {
            TempData["InfoMessage"] = "Please register as an author before creating stories.";
            return RedirectToAction(nameof(RegisterAuthor));
        }

        return View("StoryForm", new WriterStoryEditorViewModel
        {
            Title = "Create story",
            Form = new StoryFormViewModel(),
            Categories = await GetCategoriesAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStory(WriterStoryEditorViewModel editor)
    {
        var model = editor.Form ?? new StoryFormViewModel();

        if (!await HasRegisteredAuthorAsync())
        {
            TempData["InfoMessage"] = "Please register as an author before creating stories.";
            return RedirectToAction(nameof(RegisterAuthor));
        }

        if (!ModelState.IsValid)
        {
            return View("StoryForm", await BuildStoryEditorAsync("Create story", model));
        }

        var currentUserId = HttpContext.Session.GetCurrentUserId()!.Value;
        var author = await _context.Authors.FirstAsync(item => item.id_User == currentUserId);

        var imagePath = await SaveUploadedImageAsync(model.ImageFile);
        
        if (!ModelState.IsValid)
        {
            return View("StoryForm", await BuildStoryEditorAsync("Create story", model));
        }

        var story = new Story
        {
            id_Author = author.id_Author,
            StoryName = model.StoryName.Trim(),
            id_Category = model.CategoryId,
            Image = imagePath,
            Description = model.Description?.Trim(),
            PostStatus = "cho_duyet",
            Posted_At = DateTime.UtcNow,
            Modified_At = DateTime.UtcNow,
            Views = 0
        };

        _context.Stories.Add(story);
        await _context.SaveChangesAsync();

        AddNotification(currentUserId, $"Story \"{story.StoryName}\" has been created.");
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Story created successfully.";
        return RedirectToAction(nameof(Chapters), new { storyId = story.id_Story });
    }

    [HttpGet]
    public async Task<IActionResult> EditStory(int id)
    {
        if (!await HasRegisteredAuthorAsync())
        {
            TempData["InfoMessage"] = "Please register as an author before creating stories.";
            return RedirectToAction(nameof(RegisterAuthor));
        }

        var story = await GetOwnedStoryAsync(id);
        if (story is null)
        {
            return NotFound();
        }

        var model = new StoryFormViewModel
        {
            StoryId = story.id_Story,
            StoryName = story.StoryName ?? string.Empty,
            CategoryId = story.id_Category,
            CurrentImage = story.Image,
            Description = story.Description
        };

        return View("StoryForm", await BuildStoryEditorAsync("Edit story", model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditStory(int id, WriterStoryEditorViewModel editor)
    {
        var model = editor.Form ?? new StoryFormViewModel();

        if (!await HasRegisteredAuthorAsync())
        {
            TempData["InfoMessage"] = "Please register as an author before creating stories.";
            return RedirectToAction(nameof(RegisterAuthor));
        }

        if (!ModelState.IsValid)
        {
            model.StoryId = id;
            return View("StoryForm", await BuildStoryEditorAsync("Edit story", model));
        }

        var story = await GetOwnedStoryAsync(id, trackChanges: true);
        if (story is null)
        {
            return NotFound();
        }

        story.StoryName = model.StoryName.Trim();
        story.id_Category = model.CategoryId;
        
        if (model.ImageFile is not null)
        {
            var newImagePath = await SaveUploadedImageAsync(model.ImageFile);
            if (!ModelState.IsValid)
            {
                model.StoryId = id;
                return View("StoryForm", await BuildStoryEditorAsync("Edit story", model));
            }
            if (!string.IsNullOrEmpty(newImagePath))
            {
                story.Image = newImagePath;
            }
        }
        
        story.Description = model.Description?.Trim();
        story.Modified_At = DateTime.UtcNow;
        story.Reject_Reason = null;

        AddNotification(HttpContext.Session.GetCurrentUserId()!.Value, $"Story \"{story.StoryName}\" has been updated.");
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Story updated successfully.";
        return RedirectToAction(nameof(Stories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStory(int id)
    {
        if (!await HasRegisteredAuthorAsync())
        {
            TempData["InfoMessage"] = "Please register as an author before creating stories.";
            return RedirectToAction(nameof(RegisterAuthor));
        }

        var story = await GetOwnedStoryAsync(id, trackChanges: true);
        if (story is null)
        {
            return NotFound();
        }

        var chapterIds = await _context.Chapters
            .Where(item => item.id_Story == story.id_Story)
            .Select(item => item.id_Chapter)
            .ToListAsync();

        if (chapterIds.Count > 0)
        {
            var chapterAudios = await _context.ChapterAudios
                .Where(item => item.id_Chapter.HasValue && chapterIds.Contains(item.id_Chapter.Value))
                .ToListAsync();
            _context.ChapterAudios.RemoveRange(chapterAudios);
        }

        var storyComments = await _context.Comments
            .Where(item => item.id_Story == story.id_Story)
            .ToListAsync();
        _context.Comments.RemoveRange(storyComments);

        var storyFavorites = await _context.Favourites
            .Where(item => item.id_Story == story.id_Story)
            .ToListAsync();
        _context.Favourites.RemoveRange(storyFavorites);

        var storyHistory = await _context.ReadingHistories
            .Where(item => item.id_Story == story.id_Story)
            .ToListAsync();
        _context.ReadingHistories.RemoveRange(storyHistory);

        var storyChapters = await _context.Chapters
            .Where(item => item.id_Story == story.id_Story)
            .ToListAsync();
        _context.Chapters.RemoveRange(storyChapters);

        _context.Stories.Remove(story);

        AddNotification(HttpContext.Session.GetCurrentUserId()!.Value, $"Story \"{story.StoryName}\" has been deleted.");
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Story deleted successfully.";
        return RedirectToAction(nameof(Stories));
    }

    [HttpGet]
    public async Task<IActionResult> Chapters(int storyId)
    {
        if (!await HasRegisteredAuthorAsync())
        {
            TempData["InfoMessage"] = "Please register as an author before creating stories.";
            return RedirectToAction(nameof(RegisterAuthor));
        }

        var story = await GetOwnedStoryAsync(storyId);
        if (story is null)
        {
            return NotFound();
        }

        var model = new WriterStoryManageViewModel
        {
            StoryId = story.id_Story,
            StoryName = story.StoryName ?? "Untitled story",
            PostStatus = story.PostStatus,
                        RejectReason = story.Reject_Reason,
            Chapters = story.Chapters
                .OrderBy(item => item.ChapterNumber)
                .ThenBy(item => item.id_Chapter)
                .Select(item => new StoryChapterListItemViewModel
                {
                    ChapterId = item.id_Chapter,
                    ChapterNumber = item.ChapterNumber,
                    ChapterName = item.ChapterName ?? $"Chapter {item.ChapterNumber}",
                    PostedAt = item.Modified_At ?? item.Posted_At
                })
                .ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> CreateChapter(int storyId)
    {
        if (!await HasRegisteredAuthorAsync())
        {
            TempData["InfoMessage"] = "Please register as an author before creating stories.";
            return RedirectToAction(nameof(RegisterAuthor));
        }

        var story = await GetOwnedStoryAsync(storyId);
        if (story is null)
        {
            return NotFound();
        }

        return View("ChapterForm", new ChapterFormViewModel
        {
            StoryId = story.id_Story,
            StoryName = story.StoryName ?? "Untitled story"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateChapter(ChapterFormViewModel model)
    {
        if (!await HasRegisteredAuthorAsync())
        {
            TempData["InfoMessage"] = "Please register as an author before creating stories.";
            return RedirectToAction(nameof(RegisterAuthor));
        }

        var story = await GetOwnedStoryAsync(model.StoryId, trackChanges: true);
        if (story is null)
        {
            return NotFound();
        }

        model.StoryName = story.StoryName ?? "Untitled story";

        if (await HasDuplicateChapterNumberAsync(model.StoryId, model.ChapterNumber))
        {
            ModelState.AddModelError(nameof(model.ChapterNumber), "This chapter number already exists for the story.");
        }

        if (!ModelState.IsValid)
        {
            return View("ChapterForm", model);
        }

        var chapter = new Chapter
        {
            id_Story = story.id_Story,
            ChapterNumber = model.ChapterNumber,
            ChapterName = model.ChapterName.Trim(),
            Content = model.Content.Trim(),
            Posted_At = DateTime.UtcNow,
            Modified_At = DateTime.UtcNow
        };

        _context.Chapters.Add(chapter);
        story.Modified_At = DateTime.UtcNow;

        AddNotification(HttpContext.Session.GetCurrentUserId()!.Value, $"Chapter \"{chapter.ChapterName}\" has been added to \"{story.StoryName}\".");
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Chapter created successfully.";
        return RedirectToAction(nameof(Chapters), new { storyId = story.id_Story });
    }

    [HttpGet]
    public async Task<IActionResult> EditChapter(int id)
    {
        if (!await HasRegisteredAuthorAsync())
        {
            TempData["InfoMessage"] = "Please register as an author before creating stories.";
            return RedirectToAction(nameof(RegisterAuthor));
        }

        var chapter = await GetOwnedChapterAsync(id);
        if (chapter is null || chapter.Story is null)
        {
            return NotFound();
        }

        return View("ChapterForm", new ChapterFormViewModel
        {
            ChapterId = chapter.id_Chapter,
            StoryId = chapter.id_Story ?? 0,
            StoryName = chapter.Story.StoryName ?? "Untitled story",
            ChapterNumber = chapter.ChapterNumber,
            ChapterName = chapter.ChapterName ?? string.Empty,
            Content = chapter.Content ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditChapter(int id, ChapterFormViewModel model)
    {
        if (!await HasRegisteredAuthorAsync())
        {
            TempData["InfoMessage"] = "Please register as an author before creating stories.";
            return RedirectToAction(nameof(RegisterAuthor));
        }

        var chapter = await GetOwnedChapterAsync(id, trackChanges: true);
        if (chapter is null || chapter.Story is null || !chapter.id_Story.HasValue)
        {
            return NotFound();
        }

        model.StoryId = chapter.id_Story.Value;
        model.ChapterId = id;
        model.StoryName = chapter.Story.StoryName ?? "Untitled story";

        if (await HasDuplicateChapterNumberAsync(model.StoryId, model.ChapterNumber, id))
        {
            ModelState.AddModelError(nameof(model.ChapterNumber), "This chapter number already exists for the story.");
        }

        if (!ModelState.IsValid)
        {
            return View("ChapterForm", model);
        }

        chapter.ChapterNumber = model.ChapterNumber;
        chapter.ChapterName = model.ChapterName.Trim();
        chapter.Content = model.Content.Trim();
        chapter.Modified_At = DateTime.UtcNow;
        chapter.Story.Modified_At = DateTime.UtcNow;

        AddNotification(HttpContext.Session.GetCurrentUserId()!.Value, $"Chapter \"{chapter.ChapterName}\" has been updated.");
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Chapter updated successfully.";
        return RedirectToAction(nameof(Chapters), new { storyId = model.StoryId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteChapter(int id)
    {
        if (!await HasRegisteredAuthorAsync())
        {
            TempData["InfoMessage"] = "Please register as an author before creating stories.";
            return RedirectToAction(nameof(RegisterAuthor));
        }

        var chapter = await GetOwnedChapterAsync(id, trackChanges: true);
        if (chapter is null || chapter.Story is null || !chapter.id_Story.HasValue)
        {
            return NotFound();
        }

        var chapterAudios = await _context.ChapterAudios
            .Where(item => item.id_Chapter == chapter.id_Chapter)
            .ToListAsync();
        _context.ChapterAudios.RemoveRange(chapterAudios);

        var chapterReadingHistory = await _context.ReadingHistories
            .Where(item => item.id_Chapter == chapter.id_Chapter)
            .ToListAsync();
        _context.ReadingHistories.RemoveRange(chapterReadingHistory);

        _context.Chapters.Remove(chapter);
        chapter.Story.Modified_At = DateTime.UtcNow;

        AddNotification(HttpContext.Session.GetCurrentUserId()!.Value, $"Chapter \"{chapter.ChapterName}\" has been deleted.");
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Chapter deleted successfully.";
        return RedirectToAction(nameof(Chapters), new { storyId = chapter.id_Story.Value });
    }

    private async Task<WriterStoryEditorViewModel> BuildStoryEditorAsync(string title, StoryFormViewModel form)
    {
        return new WriterStoryEditorViewModel
        {
            Title = title,
            Form = form,
            Categories = await GetCategoriesAsync()
        };
    }

    private async Task<string?> SaveUploadedImageAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError("ImageFile", "Only image files (.jpg, .jpeg, .png, .gif, .webp) are allowed.");
            return null;
        }

        const long maxFileSize = 5 * 1024 * 1024; // 5 MB
        if (file.Length > maxFileSize)
        {
            ModelState.AddModelError("ImageFile", "Image file must be smaller than 5 MB.");
            return null;
        }

        try
        {
            var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "stories");
            
            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsDirectory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/stories/{fileName}";
        }
        catch (Exception)
        {
            ModelState.AddModelError("ImageFile", "Failed to save image. Please try again.");
            return null;
        }
    }

    private async Task<IReadOnlyList<CategoryLinkViewModel>> GetCategoriesAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(item => item.CategoryName)
            .Select(item => new CategoryLinkViewModel
            {
                CategoryId = item.id_Category,
                CategoryName = item.CategoryName ?? "Uncategorized"
            })
            .ToListAsync();
    }

    private async Task<bool> HasRegisteredAuthorAsync()
    {
        var author = await GetCurrentAuthorAsync();
        return author is not null && !string.IsNullOrWhiteSpace(author.PenName);
    }

    private async Task<Author?> GetCurrentAuthorAsync()
    {
        var currentUserId = HttpContext.Session.GetCurrentUserId()!.Value;
        return await _context.Authors.FirstOrDefaultAsync(item => item.id_User == currentUserId);
    }

    private async Task<Story?> GetOwnedStoryAsync(int storyId, bool trackChanges = false)
    {
        var currentUserId = HttpContext.Session.GetCurrentUserId()!.Value;
        IQueryable<Story> query = _context.Stories
            .Include(item => item.Author)
            .Include(item => item.Category)
            .Include(item => item.Chapters)
            .Where(item => item.id_Story == storyId && item.Author != null && item.Author.id_User == currentUserId);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync();
    }

    private async Task<Chapter?> GetOwnedChapterAsync(int chapterId, bool trackChanges = false)
    {
        var currentUserId = HttpContext.Session.GetCurrentUserId()!.Value;
        IQueryable<Chapter> query = _context.Chapters
            .Include(item => item.Story)
                .ThenInclude(story => story!.Author)
            .Where(item => item.id_Chapter == chapterId &&
                           item.Story != null &&
                           item.Story.Author != null &&
                           item.Story.Author.id_User == currentUserId);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync();
    }

    private async Task<bool> HasDuplicateChapterNumberAsync(int storyId, decimal? chapterNumber, int? ignoredChapterId = null)
    {
        if (!chapterNumber.HasValue)
        {
            return false;
        }

        return await _context.Chapters.AnyAsync(item =>
            item.id_Story == storyId &&
            item.ChapterNumber == chapterNumber &&
            (!ignoredChapterId.HasValue || item.id_Chapter != ignoredChapterId.Value));
    }

    private void AddNotification(int userId, string content)
    {
        _context.Notifications.Add(new Notification
        {
            id_User = userId,
            Content = content,
            IsRead = false,
            Created_At = DateTime.UtcNow
        });
    }
}
