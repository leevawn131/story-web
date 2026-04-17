using System.Net;
using Microsoft.AspNetCore.Mvc;
using story_web.Data;
using story_web.Models;

namespace story_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // LIST
        public IActionResult Index()
        {
            return View(_context.Categories.ToList());
        }

        // CREATE GET
        public IActionResult Create()
        {
            return View();
        }

        // CREATE POST
        [HttpPost]
        public IActionResult Create(Category c)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(c);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(c);
        }

        // EDIT GET
        public IActionResult Edit(int id)
        {
            var c = _context.Categories.Find(id);
            return View(c);
        }

        // EDIT POST
        [HttpPost]
        public IActionResult Edit(Category c)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(c);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(c);
        }

        // DELETE
        public IActionResult Delete(int id)
        {
            var c = _context.Categories.Find(id);
            return View(c);
        }
        //DELETE POST
        [HttpPost]
        public IActionResult DeleteConfirmed(int id_Category)
        {
            var c = _context.Categories.Find(id_Category);
            _context.Categories.Remove(c);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}