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
    public class MembershipsController : Controller
    {
        private readonly AppDbContext _context;
        public MembershipsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Plans()
        {
            var plans = _context.Memberships.ToList();
            return View(plans);
        }
        public IActionResult CreatePlan()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePlan(Membership model)
        {
            if (ModelState.IsValid)
            {
                _context.Memberships.Add(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(Plans));
            }
            return View(model);
        }
        public IActionResult EditPlans(int id)
        {
            var plan = _context.Memberships.Find(id);
            if (plan == null) return NotFound();
            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPlans(Membership model)
        {
            if (ModelState.IsValid)
            {
                _context.Memberships.Update(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(Plans));
            }
            return View(model);
        }
        public IActionResult DeletePlans(int id)
        {
            var plan = _context.Memberships.Find(id);
            if(plan == null) return NotFound();
            return View(plan);
        }
        [HttpPost, ActionName("DeletePlan")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePlanConfirmed(int id)
        {
            var plan = _context.Memberships.Find(id);
            if(plan != null)
            {
                _context.Memberships.Remove(plan);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Plans));
        }
        public IActionResult Users()
        {
            var user = _context.UserMemberships
                    .Include(u => u.User)
                    .Include(u => u.Membership)
                    .OrderByDescending(u => u.EndDate)
                    .ToList();
            return View(user);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}