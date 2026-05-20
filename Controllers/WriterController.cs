using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using story_web.Data;
using story_web.Extensions;
using story_web.Filters;
using story_web.Models;
using story_web.Services;

namespace story_web.Controllers;

[Auth]
public class WriterController : Controller
{
    private readonly AppDbContext _context;
    private readonly OllamaService _ollamaService;
    private readonly PiperService _piperService;

    public WriterController(AppDbContext context, OllamaService ollamaService, PiperService piperService)
    {
        _context = context;
        _ollamaService = ollamaService;
        _piperService = piperService;
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(Stories));
    }

    public async Task<IActionResult> Stories()
    {
        var redirect = await EnsureAuthorRegisteredAsync("Vui lòng đăng ký làm tác giả trước khi tạo truyện.");
        if (redirect is not null)
        {
            return redirect;
        }

        var currentUserId = HttpContext.Session.GetCurrentUserId()!.Value;

        var stories = await _context.Stories
            .AsNoTracking()
            .Include(item => item.Author)
            .Include(item => item.StoryCategories)
                .ThenInclude(sc => sc.Category)
            .Include(item => item.Chapters)
            .Where(item => item.Author != null &&
                           item.Author.id_User == currentUserId)
            .OrderByDescending(item => item.Modified_At ?? item.Posted_At)
            .Select(item => new WriterStoryListItemViewModel
            {
                StoryId = item.id_Story,

                StoryName = item.StoryName ?? "Untitled story",

                CategoryName = string.Join(", ",
                    item.StoryCategories
                        .Select(sc => sc.Category!.CategoryName)),

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
    public async Task<IActionResult> RegisterAuthor(
        AuthorRegistrationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUserId =
            HttpContext.Session.GetCurrentUserId()!.Value;

        var normalizedPenName = model.PenName.Trim();

        var duplicatePenNameExists = await _context.Authors
            .AsNoTracking()
            .AnyAsync(item =>
                item.id_User != currentUserId &&
                item.PenName != null &&
                item.PenName.ToLower() ==
                normalizedPenName.ToLower());

        if (duplicatePenNameExists)
        {
            ModelState.AddModelError(
                nameof(model.PenName),
                "Bút danh này đã được sử dụng.");

            return View(model);
        }

        var author = await _context.Authors
            .FirstOrDefaultAsync(item =>
                item.id_User == currentUserId);

        if (author is null)
        {
            author = new Author
            {
                id_User = currentUserId
            };

            _context.Authors.Add(author);
        }

        author.PenName = normalizedPenName;

        author.Bio = string.IsNullOrWhiteSpace(model.Bio)
            ? string.Empty
            : model.Bio.Trim();

        author.Avatar = string.IsNullOrWhiteSpace(model.Avatar)
            ? null
            : model.Avatar.Trim();

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Thông tin tác giả đã được lưu thành công.";

        return RedirectToAction(nameof(Stories));
    }

    [HttpGet]
    public async Task<IActionResult> CreateStory()
    {
        var redirect = await EnsureAuthorRegisteredAsync("Vui lòng đăng ký làm tác giả trước khi tạo truyện.");
        if (redirect is not null)
        {
            return redirect;
        }

        return View("StoryForm",
            new WriterStoryEditorViewModel
            {
                Title = "Create story",

                Form = new StoryFormViewModel(),

                Categories = await GetCategoriesAsync()
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStory(
        WriterStoryEditorViewModel editor)
    {
        var model = editor.Form ?? new StoryFormViewModel();

        var redirect = await EnsureAuthorRegisteredAsync("Please register as an author before creating stories.");
        if (redirect is not null)
        {
            return redirect;
        }

        ValidateOriginalAuthor(model);

        if (!ModelState.IsValid)
        {
            return View("StoryForm",
                await BuildStoryEditorAsync(
                    "Create story",
                    model));
        }

        var currentUserId =
            HttpContext.Session.GetCurrentUserId()!.Value;

        var author = await _context.Authors
            .FirstAsync(item =>
                item.id_User == currentUserId);

        var imagePath =
            await SaveUploadedImageAsync(model.ImageFile);

        if (!ModelState.IsValid)
        {
            return View("StoryForm",
                await BuildStoryEditorAsync(
                    "Create story",
                    model));
        }

        var story = new Story
        {
            id_Author = author.id_Author,

            StoryName = model.StoryName.Trim(),

            StoryStatus = model.StoryStatus,
            
            IsOriginal = model.IsOriginal,
            
            OriginalAuthor = model.OriginalAuthor?.Trim(),

            Image = imagePath,

            Description = model.Description?.Trim(),

            PostStatus = "cho_duyet",

            Posted_At = DateTime.UtcNow,

            Modified_At = DateTime.UtcNow,

            Views = 0
        };

        _context.Stories.Add(story);

        await _context.SaveChangesAsync();

        foreach (var categoryId in model.SelectedCategories)
        {
            _context.StoryCategories.Add(
                new StoryCategory
                {
                    id_Story = story.id_Story,

                    id_Category = categoryId
                });
        }

        AddNotification(
            currentUserId,
            $"Story \"{story.StoryName}\" has been created.");

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Truyện đã được tạo thành công.";

        return RedirectToAction(
            nameof(Chapters),
            new { storyId = story.id_Story });
    }

    [HttpGet]
    public async Task<IActionResult> EditStory(int id)
    {
        var redirect = await EnsureAuthorRegisteredAsync("Vui lòng đăng ký làm tác giả trước khi tạo truyện.");
        if (redirect is not null)
        {
            return redirect;
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

            SelectedCategories = story.StoryCategories
                .Select(sc => sc.id_Category)
                .ToList(),

            StoryStatus = story.StoryStatus,
            
            IsOriginal = story.IsOriginal,
            
            OriginalAuthor = story.OriginalAuthor,

            CurrentImage = story.Image,

            Description = story.Description
        };

        return View("StoryForm",
            await BuildStoryEditorAsync(
                "Edit story",
                model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditStory(
        int id,
        WriterStoryEditorViewModel editor)
    {
        var model = editor.Form ?? new StoryFormViewModel();

        var redirect = await EnsureAuthorRegisteredAsync("Vui lòng đăng ký làm tác giả trước khi tạo truyện.");
        if (redirect is not null)
        {
            return redirect;
        }

        ValidateOriginalAuthor(model);

        if (!ModelState.IsValid)
        {
            model.StoryId = id;

            return View("StoryForm",
                await BuildStoryEditorAsync(
                    "Edit story",
                    model));
        }

        var story = await GetOwnedStoryAsync(
            id,
            trackChanges: true);

        if (story is null)
        {
            return NotFound();
        }

        story.StoryName = model.StoryName.Trim();

        story.StoryStatus = model.StoryStatus;
        
        story.IsOriginal = model.IsOriginal;
        
        story.OriginalAuthor = model.OriginalAuthor?.Trim();

        if (model.ImageFile is not null)
        {
            var newImagePath =
                await SaveUploadedImageAsync(model.ImageFile);

            if (!ModelState.IsValid)
            {
                model.StoryId = id;

                return View("StoryForm",
                    await BuildStoryEditorAsync(
                        "Edit story",
                        model));
            }

            if (!string.IsNullOrEmpty(newImagePath))
            {
                story.Image = newImagePath;
            }
        }

        story.Description = model.Description?.Trim();

        story.Modified_At = DateTime.UtcNow;

        story.Reject_Reason = null;

        var oldCategories = await _context.StoryCategories
            .Where(sc => sc.id_Story == story.id_Story)
            .ToListAsync();

        _context.StoryCategories.RemoveRange(oldCategories);

        foreach (var categoryId in model.SelectedCategories)
        {
            _context.StoryCategories.Add(
                new StoryCategory
                {
                    id_Story = story.id_Story,

                    id_Category = categoryId
                });
        }

        AddNotification(
            HttpContext.Session.GetCurrentUserId()!.Value,
            $"Story \"{story.StoryName}\" has been updated.");

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Truyện đã được cập nhật thành công.";

        return RedirectToAction(nameof(Stories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStory(int id)
    {
        var redirect = await EnsureAuthorRegisteredAsync("Vui lòng đăng ký làm tác giả trước khi tạo truyện.");
        if (redirect is not null)
        {
            return redirect;
        }

        var story = await GetOwnedStoryAsync(id, trackChanges: true);
        if (story is null)
        {
            return NotFound();
        }

        var readingHistories = await _context.ReadingHistories
            .Where(item => item.id_Story == story.id_Story)
            .ToListAsync();

        var favourites = await _context.Favourites
            .Where(item => item.id_Story == story.id_Story)
            .ToListAsync();

        var comments = await _context.Comments
            .Where(item => item.id_Story == story.id_Story)
            .ToListAsync();

        var storyCategories = await _context.StoryCategories
            .Where(item => item.id_Story == story.id_Story)
            .ToListAsync();

        _context.ReadingHistories.RemoveRange(readingHistories);
        _context.Favourites.RemoveRange(favourites);
        _context.Comments.RemoveRange(comments);
        _context.StoryCategories.RemoveRange(storyCategories);
        _context.Chapters.RemoveRange(story.Chapters);
        _context.Stories.Remove(story);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Truyện đã được xóa thành công.";
        return RedirectToAction(nameof(Stories));
    }

    private async Task<WriterStoryEditorViewModel>
        BuildStoryEditorAsync(
            string title,
            StoryFormViewModel form)
    {
        return new WriterStoryEditorViewModel
        {
            Title = title,

            Form = form,

            Categories = await GetCategoriesAsync()
        };
    }

    private async Task<IReadOnlyList<CategoryLinkViewModel>>
        GetCategoriesAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(item => item.CategoryName)
            .Select(item => new CategoryLinkViewModel
            {
                CategoryId = item.id_Category,

                CategoryName =
                    item.CategoryName ?? "Uncategorized"
            })
            .ToListAsync();
    }

    private async Task<bool> HasRegisteredAuthorAsync()
    {
        var author = await GetCurrentAuthorAsync();

        return author is not null
            && !string.IsNullOrWhiteSpace(author.PenName)
            && author.Bio is not null;
    }

    private async Task<Author?> GetCurrentAuthorAsync()
    {
        var currentUserId =
            HttpContext.Session.GetCurrentUserId()!.Value;

        return await _context.Authors
            .FirstOrDefaultAsync(item =>
                item.id_User == currentUserId);
    }

    private async Task<Story?> GetOwnedStoryAsync(
        int storyId,
        bool trackChanges = false)
    {
        var currentUserId =
            HttpContext.Session.GetCurrentUserId()!.Value;

        IQueryable<Story> query = _context.Stories

            .Include(item => item.Author)

            .Include(item => item.StoryCategories)
                .ThenInclude(sc => sc.Category)

            .Include(item => item.Chapters)

            .Where(item =>
                item.id_Story == storyId &&
                item.Author != null &&
                item.Author.id_User == currentUserId);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync();
    }
    private async Task<Chapter?> GetOwnedChapterAsync(int chapterId, bool trackChanges = false)
    {
        var currentUserId =
            HttpContext.Session.GetCurrentUserId()!.Value;

        IQueryable<Chapter> query = _context.Chapters
            .Include(item => item.Story)
                .ThenInclude(story => story!.Author)
            .Where(item =>
                item.id_Chapter == chapterId &&
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

    [HttpGet]
    public async Task<IActionResult> CreateChapter(int storyId)
    {
        var redirect = await EnsureAuthorRegisteredAsync("Vui lòng đăng ký làm tác giả trước khi tạo truyện.");
        if (redirect is not null)
        {
            return redirect;
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
        var redirect = await EnsureAuthorRegisteredAsync("Vui lòng đăng ký làm tác giả trước khi tạo truyện.");
        if (redirect is not null)
        {
            return redirect;
        }

        var story = await GetOwnedStoryAsync(model.StoryId, trackChanges: true);
        if (story is null)
        {
            return NotFound();
        }

        model.StoryName = story.StoryName ?? "Untitled story";

        if (await HasDuplicateChapterNumberAsync(model.StoryId, model.ChapterNumber))
        {
            ModelState.AddModelError(nameof(model.ChapterNumber), "Số chương này đã tồn tại cho truyện.");
        }

        if (!ModelState.IsValid)
        {
            return View("ChapterForm", model);
        }

        var chapter = new Chapter
        {
            id_Story = story.id_Story,
            ChapterNumber = model.ChapterNumber,
            ChapterName = model.ChapterName?.Trim(),
            Content = model.Content?.Trim(),
            Posted_At = DateTime.UtcNow,
            Modified_At = DateTime.UtcNow
        };

        _context.Chapters.Add(chapter);
        story.Modified_At = DateTime.UtcNow;

        AddNotification(HttpContext.Session.GetCurrentUserId()!.Value, $"Chương \"{chapter.ChapterName}\" đã được thêm vào \"{story.StoryName}\".");
        await _context.SaveChangesAsync();

        // Notify users who favourited this story (use existing Favourites table).
        try
        {
            int? currentUserId = HttpContext.Session.GetCurrentUserId();

            var favouriteUserIds = await _context.Favourites
                .Where(f => f.id_Story == story.id_Story && f.id_User.HasValue)
                .Select(f => f.id_User!.Value)
                .Distinct()
                .ToListAsync();

            foreach (var favUserId in favouriteUserIds)
            {
                // don't notify the actor who created the chapter
                if (currentUserId.HasValue && favUserId == currentUserId.Value)
                {
                    continue;
                }

                _context.Notifications.Add(new Notification
                {
                    id_User = favUserId,
                    Content = $"Tác giả {story.Author?.PenName ?? "tác giả"} vừa thêm chương mới \"{chapter.ChapterName}\" vào \"{story.StoryName}\".",
                    IsRead = false,
                    Created_At = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }
        catch
        {
            // swallow any notification errors to avoid blocking chapter creation
        }

        chapter.AISummary = await _ollamaService.SummarizeAsync(chapter.Content ?? string.Empty);
        await _context.SaveChangesAsync();

        chapter.AudioPath = await _piperService.GenerateAudioAsync(chapter.Content ?? string.Empty, chapter.id_Chapter);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Chương đã được tạo thành công.";
        return RedirectToAction(nameof(Chapters), new { storyId = story.id_Story });
    }

    [HttpGet]
    public async Task<IActionResult> EditChapter(int id)
    {
        var redirect = await EnsureAuthorRegisteredAsync("Vui lòng đăng ký làm tác giả trước khi tạo truyện.");
        if (redirect is not null)
        {
            return redirect;
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
        var redirect = await EnsureAuthorRegisteredAsync("Vui lòng đăng ký làm tác giả trước khi tạo truyện.");
        if (redirect is not null)
        {
            return redirect;
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
            ModelState.AddModelError(nameof(model.ChapterNumber), "Số chương này đã tồn tại cho truyện.");
        }

        if (!ModelState.IsValid)
        {
            return View("ChapterForm", model);
        }

        chapter.ChapterNumber = model.ChapterNumber;
        chapter.ChapterName = model.ChapterName?.Trim();
        chapter.Content = model.Content?.Trim();
        chapter.Modified_At = DateTime.UtcNow;
        chapter.Story.Modified_At = DateTime.UtcNow;

        AddNotification(HttpContext.Session.GetCurrentUserId()!.Value, $"Chương \"{chapter.ChapterName}\" đã được cập nhật.");
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Chương đã được cập nhật thành công.";
        return RedirectToAction(nameof(Chapters), new { storyId = model.StoryId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteChapter(int id)
    {
        var redirect = await EnsureAuthorRegisteredAsync("Vui lòng đăng ký làm tác giả trước khi tạo truyện.");
        if (redirect is not null)
        {
            return redirect;
        }

        var chapter = await GetOwnedChapterAsync(id, trackChanges: true);
        if (chapter is null || chapter.Story is null || !chapter.id_Story.HasValue)
        {
            return NotFound();
        }

        var chapterReadingHistory = await _context.ReadingHistories
            .Where(item => item.id_Chapter == chapter.id_Chapter)
            .ToListAsync();
        _context.ReadingHistories.RemoveRange(chapterReadingHistory);

        _context.Chapters.Remove(chapter);
        chapter.Story.Modified_At = DateTime.UtcNow;

        AddNotification(HttpContext.Session.GetCurrentUserId()!.Value, $"Chương \"{chapter.ChapterName}\" đã bị xóa.");
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Chương đã được xóa thành công.";
        return RedirectToAction(nameof(Chapters), new { storyId = chapter.id_Story.Value });
    }

    private async Task<string?> SaveUploadedImageAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        var allowedExtensions =
            new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        var extension =
            Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError(
                "ImageFile",
                "Chỉ cho phép file ảnh.");

            return null;
        }

        var uploadsDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "stories");

        if (!Directory.Exists(uploadsDirectory))
        {
            Directory.CreateDirectory(uploadsDirectory);
        }

        var fileName =
            $"{Guid.NewGuid()}{extension}";

        var filePath =
            Path.Combine(uploadsDirectory, fileName);

        using (var stream = new FileStream(
            filePath,
            FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/stories/{fileName}";
    }

    private void AddNotification(
        int userId,
        string content)
    {
        _context.Notifications.Add(
            new Notification
            {
                id_User = userId,

                Content = content,

                IsRead = false,

                Created_At = DateTime.UtcNow
            });
    }
    [HttpGet]
    public async Task<IActionResult> Chapters(int storyId)
    {
        var story = await GetOwnedStoryAsync(storyId);
        if (story is null)
        {
            return NotFound();
        }

        var model = new WriterStoryManageViewModel
        {
            StoryId = story.id_Story,
            StoryName = story.StoryName ?? "Truyện chưa có tên",
            PostStatus = story.PostStatus,
            RejectReason = story.Reject_Reason,
            SelectedCategories = story.StoryCategories
                .Select(sc => sc.id_Category)
                .ToList(),
            Chapters = story.Chapters
                .OrderByDescending(c => c.ChapterNumber)
                .Select(c => new StoryChapterListItemViewModel
                {
                    ChapterId = c.id_Chapter,
                    ChapterNumber = c.ChapterNumber,
                    ChapterName = c.ChapterName ?? string.Empty,
                    PostedAt = c.Posted_At
                })
                .ToList()
        };

        return View(model);
    }

    private async Task<IActionResult?> EnsureAuthorRegisteredAsync(string message)
    {
        if (await HasRegisteredAuthorAsync())
        {
            return null;
        }

        TempData["InfoMessage"] = message;
        return RedirectToAction(nameof(RegisterAuthor));
    }

    private void ValidateOriginalAuthor(StoryFormViewModel model)
    {
        if (!model.IsOriginal && string.IsNullOrWhiteSpace(model.OriginalAuthor))
        {
            ModelState.AddModelError("Form.OriginalAuthor", "Vui lòng nhập tên tác giả gốc cho truyện sưu tầm/dịch.");
        }

        if (model.IsOriginal)
        {
            model.OriginalAuthor = null;
        }
    }
}