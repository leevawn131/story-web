using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using story_web.Data;
using story_web.Filters;
using story_web.Models;

namespace story_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Auth(UserRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);
            if(user != null)
            {
                if(user.Role == 1)
                {
                    return Content("Can not delete Admin");
                }
                _context.Users.Remove(user);
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