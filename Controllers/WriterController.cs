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
            TempData["InfoMessage"] = "Vui lòng đăng ký làm tác giả trước khi tạo truyện.";
            return RedirectToAction(nameof(RegisterAuthor));
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
        if (!await HasRegisteredAuthorAsync())
        {
            TempData["InfoMessage"] =
                "Vui lòng đăng ký làm tác giả trước khi tạo truyện.";

            return RedirectToAction(nameof(RegisterAuthor));
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

        if (!await HasRegisteredAuthorAsync())
        {
            TempData["InfoMessage"] =
                "Please register as an author before creating stories.";

            return RedirectToAction(nameof(RegisterAuthor));
        }

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
        if (!await HasRegisteredAuthorAsync())
        {
            TempData["InfoMessage"] =
                "Vui lòng đăng ký làm tác giả trước khi tạo truyện.";

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

            SelectedCategories = story.StoryCategories
                .Select(sc => sc.id_Category)
                .ToList(),

            StoryStatus = story.StoryStatus,

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

        if (!await HasRegisteredAuthorAsync())
        {
            TempData["InfoMessage"] =
                "Vui lòng đăng ký làm tác giả trước khi tạo truyện.";

            return RedirectToAction(nameof(RegisterAuthor));
        }

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
[HttpGet]
public async Task<IActionResult> Chapters(int storyId)
{
    // 1. Lấy thông tin truyện để đảm bảo người dùng này là chủ sở hữu
    var story = await GetOwnedStoryAsync(storyId);

    // Nếu không tìm thấy truyện, trả về trang 404
    if (story is null)
    {
        return NotFound();
    }

    // 2. Tạo Model để truyền sang View
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

    // 3. Truyền model vào View
    return View(model); 
}
}