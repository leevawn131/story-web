using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using story_web.Data;
using story_web.Filters;
using story_web.Models;

namespace story_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Auth(UserRoles.Admin)]
    public class PaymentsController : Controller
    {
        private readonly AppDbContext _context;
        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var query = _context.Payments
                        .Include(p => p.User)
                        .Include(p => p.Membership)
                        .OrderByDescending(p => p.Created_At)
                        .ToList();
            return View(query);
        }
        public IActionResult Details(int id)
        {
            var payment = _context.Payments
                        .Include(p => p.User)
                        .Include(p => p.Membership)
                        .FirstOrDefault(p => p.id_Payment == id);
            if (payment == null)
            {
                return NotFound();
            }
            return View(payment);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var payment = _context.Payments.Find(id);
            if(payment != null)
            {
                _context.Payments.Remove(payment);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}