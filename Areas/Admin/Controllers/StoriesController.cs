using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using story_web.Data;
using story_web.Models;

namespace story_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StoriesController : Controller
    {
        private readonly AppDbContext _context;

        public StoriesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var stories = _context.Stories
            .Include(s => s.Author)
            .ToList();
            return View(stories);
        }
    }
}