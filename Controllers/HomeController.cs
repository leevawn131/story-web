using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using story_web.Data;
using story_web.Extensions;
using story_web.Filters;
using story_web.Models;

namespace story_web.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? searchTerm = null, int? categoryId = null)
    {
        var model = await BuildHomeIndexViewModelAsync(searchTerm, categoryId, "Kho truyện");
        var topViews = await _context.Stories
        // bảng xếp hạng yêu thích
        .AsNoTracking()
        .OrderByDescending(s => s.Views)
        .Take(5)
        .Select(s => new RankingItemViewModel
        {
            StoryId = s.id_Story,
            StoryName = s.StoryName!,
            ImageUrl = s.Image,
            Views = s.Views ?? 0,
            FavouriteCount = _context.Favourites.Count(f => f.id_Story == s.id_Story)
        })
        .ToListAsync();
        // bảng xếp hạng lượt xem
        var topFavourites = await _context.Stories
        .AsNoTracking()
        .OrderByDescending(s => _context.Favourites.Count(f => f.id_Story == s.id_Story))
        .Take(5)
        .Select(s => new RankingItemViewModel
        {
            StoryId = s.id_Story,
            StoryName = s.StoryName!,
            ImageUrl = s.Image,
            Views = s.Views ?? 0,
            FavouriteCount = _context.Favourites.Count(f => f.id_Story == s.id_Story)
        })
        
        .ToListAsync();
        // phần truyện mới cập nhật
        var latestUpdatedStories = await _context.Stories
        .AsNoTracking()
        .Include(s => s.Author)
            .ThenInclude(a => a!.User)
        .Include(s => s.Category)
        .Include(s => s.Chapters)
        .OrderByDescending(s => s.Modified_At ?? s.Posted_At)
        .Take(8)
        .Select(s => new StoryCardViewModel
        {
            StoryId = s.id_Story,
            StoryName = s.StoryName ?? "Untitled",
            ImageUrl = s.Image,
            AuthorName = s.Author!.PenName ?? s.Author.User!.UserName,
            CategoryName = s.Category!.CategoryName,
            ChapterCount = s.Chapters.Count,
            Views = s.Views ?? 0
        })
        .ToListAsync();
        model.LatestUpdatedStories = latestUpdatedStories;
        model.TopViews = topViews;
        model.TopFavourites = topFavourites;
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();

    }

    public async Task<IActionResult> Category(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.id_Category == id);

        if (category is null)
        {
            return NotFound();
        }

        var model = await BuildHomeIndexViewModelAsync(null, id, $"Thể loại: {category.CategoryName}");
        return View(model);
    }

    public async Task<IActionResult> Story(int id)
    {
        var story = await _context.Stories
            .AsNoTracking()
            .Include(item => item.Author)
                .ThenInclude(author => author!.User)
            .Include(item => item.Category)
            .Include(item => item.Chapters)
            .FirstOrDefaultAsync(item => item.id_Story == id);

        if (story is null)
        {
            return NotFound();
        }

        var currentUserId = HttpContext.Session.GetCurrentUserId();
        var isFavourite = false;
        var continueReadingChapterId = default(int?);

        if (currentUserId.HasValue)
        {
            isFavourite = await _context.Favourites
                .AsNoTracking()
                .AnyAsync(item => item.id_User == currentUserId.Value && item.id_Story == id);

            continueReadingChapterId = await _context.ReadingHistories
                .AsNoTracking()
                .Where(item => item.id_User == currentUserId.Value && item.id_Story == id)
                .OrderByDescending(item => item.Last_Read_At)
                .Select(item => item.id_Chapter)
                .FirstOrDefaultAsync();
        }

        var favouriteCount = await _context.Favourites
            .AsNoTracking()
            .CountAsync(item => item.id_Story == id);
        //phần bình luận
        var comments = await _context.Comments
    .Where(c => c.id_Story == id)
    .Include(c => c.User)
    .OrderByDescending(c => c.Posted_At)
    .Select(c => new CommentItemViewModel
    {
        CommentId = c.id_Comment,
        UserName = c.User!.UserName ?? "Ẩn danh",
        Content = c.Content,
        PostedAt = c.Posted_At
    })
    .ToListAsync();

        var model = new StoryDetailsViewModel
        {
            StoryId = story.id_Story,
            StoryName = story.StoryName ?? "Untitled story",
            Description = story.Description,
            ImageUrl = story.Image,
            AuthorName = story.Author?.PenName ?? story.Author?.User?.UserName ?? "Unknown author",
            CategoryName = story.Category?.CategoryName,
            PostStatus = story.PostStatus,
            PostedAt = story.Posted_At,
            ModifiedAt = story.Modified_At,
            Views = story.Views ?? 0,
            FavouriteCount = favouriteCount,
            IsFavourite = isFavourite,
            ContinueReadingChapterId = continueReadingChapterId,
            CanManage = currentUserId.HasValue && story.Author?.id_User == currentUserId.Value,
            Chapters = story.Chapters
                .OrderBy(item => item.ChapterNumber)
                .ThenBy(item => item.id_Chapter)
                .Select(item => new StoryChapterListItemViewModel
                {
                    ChapterId = item.id_Chapter,
                    ChapterNumber = item.ChapterNumber,
                    ChapterName = item.ChapterName ?? $"Chapter {item.ChapterNumber}",
                    PostedAt = item.Posted_At
                })
                .ToList(),
                Comments = comments

        };

        return View(model);
    }

    public async Task<IActionResult> Chapter(int id)
    {
        var chapter = await _context.Chapters
            .Include(item => item.Story)
                .ThenInclude(story => story!.Author)
                    .ThenInclude(author => author!.User)
            .FirstOrDefaultAsync(item => item.id_Chapter == id);

        if (chapter is null || chapter.Story is null || !chapter.id_Story.HasValue)
        {
            return NotFound();
        }

        var chapterList = await _context.Chapters
            .AsNoTracking()
            .Where(item => item.id_Story == chapter.id_Story)
            .OrderBy(item => item.ChapterNumber)
            .ThenBy(item => item.id_Chapter)
            .Select(item => new StoryChapterListItemViewModel
            {
                ChapterId = item.id_Chapter,
                ChapterNumber = item.ChapterNumber,
                ChapterName = item.ChapterName ?? $"Chapter {item.ChapterNumber}",
                PostedAt = item.Posted_At
            })
            .ToListAsync();

        var currentIndex = chapterList.FindIndex(item => item.ChapterId == chapter.id_Chapter);
        var previousChapterId = currentIndex > 0 ? chapterList[currentIndex - 1].ChapterId : default(int?);
        var nextChapterId = currentIndex >= 0 && currentIndex < chapterList.Count - 1 ? chapterList[currentIndex + 1].ChapterId : default(int?);

        await IncrementStoryViewsAsync(chapter.id_Story.Value);
        await UpdateReadingHistoryAsync(chapter.id_Story.Value, chapter.id_Chapter);

        var model = new ChapterReaderViewModel
        {
            StoryId = chapter.id_Story.Value,
            StoryName = chapter.Story.StoryName ?? "Untitled story",
            AuthorName = chapter.Story.Author?.PenName ?? chapter.Story.Author?.User?.UserName ?? "Unknown author",
            ChapterId = chapter.id_Chapter,
            ChapterNumber = chapter.ChapterNumber,
            ChapterName = chapter.ChapterName ?? $"Chapter {chapter.ChapterNumber}",
            Content = chapter.Content ?? string.Empty,
            PreviousChapterId = previousChapterId,
            NextChapterId = nextChapterId,
            Chapters = chapterList
        };

        return View(model);
    }
    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddComment(int storyId, string content)
{
    var userId = HttpContext.Session.GetCurrentUserId();
    if (!userId.HasValue)
        return RedirectToAction("Login", "Account");

    var comment = new Comment
    {
        id_Story = storyId,
        id_User = userId.Value,
        Content = content,
        Posted_At = DateTime.Now
    };

    _context.Comments.Add(comment);
    await _context.SaveChangesAsync();

    return RedirectToAction("Story", new { id = storyId });
}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Auth]
    public async Task<IActionResult> ToggleFavourite(int storyId, string? returnUrl = null)
    {
        var currentUserId = HttpContext.Session.GetCurrentUserId();
        if (!currentUserId.HasValue)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Story), new { id = storyId }) });
        }

        var story = await _context.Stories
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.id_Story == storyId);

        if (story is null)
        {
            return NotFound();
        }

        var favourite = await _context.Favourites
            .FirstOrDefaultAsync(item => item.id_User == currentUserId.Value && item.id_Story == storyId);

        if (favourite is null)
        {
            _context.Favourites.Add(new Favourite
            {
                id_User = currentUserId.Value,
                id_Story = storyId,
                Added_At = DateTime.UtcNow
            });

            TempData["SuccessMessage"] = $"\"{story.StoryName}\" đã được thêm vào yêu thích của bạn.";
        }
        else
        {
            _context.Favourites.Remove(favourite);
            TempData["SuccessMessage"] = $"\"{story.StoryName}\" đã được xóa khỏi yêu thích của bạn.";
        }

        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Story), new { id = storyId });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task<HomeIndexViewModel> BuildHomeIndexViewModelAsync(string? searchTerm, int? categoryId, string title)
    {
        var currentUserId = HttpContext.Session.GetCurrentUserId();
        var normalizedSearch = searchTerm?.Trim();

        var query = _context.Stories
            .AsNoTracking()
            .Include(item => item.Author)
            .ThenInclude(author => author!.User)
            .Include(item => item.Category)
            .Include(item => item.Chapters)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(item =>
                (item.StoryName != null && item.StoryName.Contains(normalizedSearch)) ||
                (item.Author != null && item.Author.PenName != null && item.Author.PenName.Contains(normalizedSearch)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(item => item.id_Category == categoryId.Value);
        }

        var stories = await query
            .OrderByDescending(item => item.Modified_At ?? item.Posted_At)
            .ThenByDescending(item => item.id_Story)
            .Take(24)
            .ToListAsync();

        var favouriteStoryIds = new HashSet<int>();
        var lastReadChapterIds = new Dictionary<int, int?>();

        if (currentUserId.HasValue && stories.Count > 0)
        {
            var storyIds = stories.Select(item => item.id_Story).ToList();

            favouriteStoryIds = await _context.Favourites
                .AsNoTracking()
                .Where(item => item.id_User == currentUserId.Value && item.id_Story.HasValue && storyIds.Contains(item.id_Story.Value))
                .Select(item => item.id_Story!.Value)
                .ToHashSetAsync();

            var readingHistory = await _context.ReadingHistories
                .AsNoTracking()
                .Where(item => item.id_User == currentUserId.Value && item.id_Story.HasValue && storyIds.Contains(item.id_Story.Value))
                .OrderByDescending(item => item.Last_Read_At)
                .ToListAsync();

            lastReadChapterIds = readingHistory
                .GroupBy(item => item.id_Story!.Value)
                .ToDictionary(group => group.Key, group => group.First().id_Chapter);
        }

        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(item => item.CategoryName)
            .Select(item => new CategoryLinkViewModel
            {
                CategoryId = item.id_Category,
                CategoryName = item.CategoryName ?? "Uncategorized",
                IsActive = categoryId.HasValue && categoryId.Value == item.id_Category
            })
            .ToListAsync();

        return new HomeIndexViewModel
        {
            SearchTerm = normalizedSearch,
            CategoryId = categoryId,
            Title = title,
            Categories = categories,
            Stories = stories.Select(item => new StoryCardViewModel
            {
                StoryId = item.id_Story,
                StoryName = item.StoryName ?? "Untitled story",
                Description = item.Description,
                ImageUrl = item.Image,
                AuthorName = item.Author?.PenName ?? item.Author?.User?.UserName ?? "Unknown author",
                CategoryName = item.Category?.CategoryName,
                PostStatus = item.PostStatus,
                ChapterCount = item.Chapters.Count,
                Views = item.Views ?? 0,
                Rating = item.Rating,
                IsFavourite = favouriteStoryIds.Contains(item.id_Story),
                LastReadChapterId = lastReadChapterIds.TryGetValue(item.id_Story, out var chapterId) ? chapterId : null
            }).ToList()
        };
    }

    private async Task IncrementStoryViewsAsync(int storyId)
    {
        var story = await _context.Stories.FirstOrDefaultAsync(item => item.id_Story == storyId);
        if (story is null)
        {
            return;
        }

        story.Views = (story.Views ?? 0) + 1;
        await _context.SaveChangesAsync();
    }

    private async Task UpdateReadingHistoryAsync(int storyId, int chapterId)
    {
        var currentUserId = HttpContext.Session.GetCurrentUserId();
        if (!currentUserId.HasValue)
        {
            return;
        }

        var history = await _context.ReadingHistories
            .FirstOrDefaultAsync(item => item.id_User == currentUserId.Value && item.id_Story == storyId);

        if (history is null)
        {
            history = new ReadingHistory
            {
                id_User = currentUserId.Value,
                id_Story = storyId
            };

            _context.ReadingHistories.Add(history);
        }

        history.id_Chapter = chapterId;
        history.Last_Read_At = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}
