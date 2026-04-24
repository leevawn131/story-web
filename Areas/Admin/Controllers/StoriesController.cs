using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using story_web.Data;
using story_web.Filters;
using story_web.Models;

namespace story_web.Areas.Admin.Controllers;

[Area("Admin")]
[Auth(UserRoles.Admin)]
public class StoriesController : Controller
{
    private readonly AppDbContext _context;

    public StoriesController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(string? status = null)
    {
        var query = _context.Stories.Include(s => s.Author).AsQueryable();

        if (status == "cho_duyet" || status == "da_duyet" || status == "tu_choi")
        {
            query = query.Where(s => s.PostStatus == status);
        }

        var stories = query
            .OrderByDescending(s => s.Posted_At)
            .ToList();

        ViewData["CurrentStatus"] = status;
        return View(stories);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int id)
    {
        var story = await _context.Stories.FindAsync(id);
        if (story is null)
            return NotFound();

        story.PostStatus = "da_duyet";
        story.Modified_At = DateTime.UtcNow;
        
        _context.Stories.Update(story);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Story '{story.StoryName}' has been approved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Reject(int id, string? reason = null)
    {
        var story = await _context.Stories.FindAsync(id);
        if (story is null)
            return NotFound();

        story.PostStatus = "tu_choi";
        story.Reject_Reason = reason;
        story.Modified_At = DateTime.UtcNow;
        
        _context.Stories.Update(story);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Story '{story.StoryName}' has been rejected.";
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public IActionResult Delete(int id)
    {
        var story = _context.Stories.Find(id);
        if (story is null)
        {
            return NotFound();
        }

        return View(story);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id_Story)
    {
        var story = _context.Stories.Find(id_Story);
        var c = _context.Chapters.Where(x => x.id_Story == id_Story).ToList();
        var read = _context.ReadingHistories.Where(x => x.id_Story == id_Story).ToList();
        var love = _context.Favourites.Where(x => x.id_Story == id_Story).ToList();
        if (story is null)
        {
            return NotFound();
        }
        
        _context.ReadingHistories.RemoveRange(read);
        _context.Favourites.RemoveRange(love);
        _context.Chapters.RemoveRange(c);
        _context.Stories.Remove(story);
        _context.SaveChanges();
        TempData["SuccessMessage"] = "Xoa truyện thanh cong.";
        return RedirectToAction(nameof(Index));
    }
}
