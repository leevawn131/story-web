using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using story_web.Data;
using story_web.Extensions;
using story_web.Filters;
using story_web.Models;

namespace story_web.Controllers;

[Auth]
public class LibraryController : Controller
{
    private readonly AppDbContext _context;

    public LibraryController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(History));
    }

    public async Task<IActionResult> History()
    {
        var currentUserId = HttpContext.Session.GetCurrentUserId()!.Value;

        var items = await _context.ReadingHistories
            .AsNoTracking()
            .Include(item => item.Story)
            .Include(item => item.Chapter)
            .Where(item => item.id_User == currentUserId)
            .OrderByDescending(item => item.Last_Read_At)
            .Select(item => new ProfileStoryItemViewModel
            {
                StoryId = item.id_Story ?? 0,
                ChapterId = item.id_Chapter,
                StoryName = item.Story != null ? (item.Story.StoryName ?? "Untitled story") : "Unknown story",
                PostStatus = item.Story != null ? item.Story.PostStatus : null,
                Subtitle = item.Chapter != null
                    ? $"Chapter {item.Chapter.ChapterNumber:0.##}: {item.Chapter.ChapterName}"
                    : "Recently read",
                ActivityAt = item.Last_Read_At
            })
            .ToListAsync();

        return View("List", new LibraryPageViewModel
        {
            Title = "Reading history",
            EmptyMessage = "You have not opened any chapter yet.",
            Items = items
        });
    }

    public async Task<IActionResult> Favorites()
    {
        var currentUserId = HttpContext.Session.GetCurrentUserId()!.Value;

        var items = await _context.Favourites
            .AsNoTracking()
            .Include(item => item.Story)
            .Where(item => item.id_User == currentUserId)
            .OrderByDescending(item => item.Added_At)
            .Select(item => new ProfileStoryItemViewModel
            {
                StoryId = item.id_Story ?? 0,
                StoryName = item.Story != null ? (item.Story.StoryName ?? "Untitled story") : "Unknown story",
                PostStatus = item.Story != null ? item.Story.PostStatus : null,
                Subtitle = "Saved to favorites",
                ActivityAt = item.Added_At
            })
            .ToListAsync();

        return View("List", new LibraryPageViewModel
        {
            Title = "Favorite stories",
            EmptyMessage = "You have not added any favorite stories yet.",
            Items = items
        });
    }

    public async Task<IActionResult> Notifications()
    {
        var currentUserId = HttpContext.Session.GetCurrentUserId()!.Value;

        var items = await _context.Notifications
            .AsNoTracking()
            .Where(item => item.id_User == currentUserId)
            .OrderByDescending(item => item.Created_At)
            .Select(item => new NotificationItemViewModel
            {
                NotificationId = item.id_Noti,
                Content = item.Content ?? string.Empty,
                IsRead = item.IsRead ?? false,
                CreatedAt = item.Created_At
            })
            .ToListAsync();

        return View(new NotificationsPageViewModel
        {
            Items = items
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkNotificationRead(int id, string? returnUrl = null)
    {
        var currentUserId = HttpContext.Session.GetCurrentUserId()!.Value;
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(item => item.id_Noti == id && item.id_User == currentUserId);

        if (notification is not null && notification.IsRead != true)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Notifications));
    }
}
