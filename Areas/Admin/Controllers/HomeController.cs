using Microsoft.AspNetCore.Mvc;
using story_web.Data;
using story_web.Filters;
using story_web.Models;

namespace story_web.Areas.Admin.Controllers;

[Area("Admin")]
[Auth(UserRoles.Admin)]
public class HomeController : Controller
{
    private readonly AppDbContext _context;
    public HomeController(AppDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        ViewBag.ToTalStories = _context.Stories.Count();
        ViewBag.TotalUsers = _context.Users.Count();
        ViewBag.PendingStories = _context.Stories.Count(s => s.PostStatus == "cho_duyet");
        ViewBag.TotalChapters = _context.Chapters.Count();
        ViewBag.RecentStories = _context.Stories.OrderByDescending(s => s.Posted_At).Take(5).ToList();
        ViewBag.TopAuthors = _context.Authors.Select(a => new
        {
            a.PenName, StoryCount = _context.Stories.Count(s => s.id_Author == a.id_Author)
        }).OrderByDescending(a => a.StoryCount).Take(5).ToList();
        
        return View();
    }
}
