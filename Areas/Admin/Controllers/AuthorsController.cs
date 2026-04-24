using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using story_web.Data;
using story_web.Filters;
using story_web.Models;

namespace story_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Auth(UserRoles.Admin)]
    public class AuthorsController : Controller
    {
        private readonly AppDbContext _context;

        public AuthorsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var author = _context.Authors.Include(a => a.User).ToList();
            return View(author);
        }
        public IActionResult Delete(int id)
        {
            var a = _context.Authors.Find(id);
            if(a != null)
            {
                _context.Authors.Remove(a);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}