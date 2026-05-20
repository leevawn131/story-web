using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using story_web.Data;
using story_web.Extensions;
using story_web.Filters;
using story_web.Models;

namespace story_web.Controllers
{
    public class MembershipController : Controller
    {
        private readonly AppDbContext _context;

        public MembershipController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var plans = await _context.Memberships
                .AsNoTracking()
                .OrderBy(m => m.Price)
                .ToListAsync();

            return View(plans);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Auth]
        public async Task<IActionResult> Buy(int id)
        {
            var userId = HttpContext.Session.GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index") });
            }

            var plan = await _context.Memberships.FindAsync(id);
            if (plan == null)
            {
                return NotFound();
            }

            // Check if user already has an active membership, maybe just extend it,
            // but for simplicity, we'll create a new record or update the existing active one.
            var existingMembership = await _context.UserMemberships
                .Where(m => m.id_User == userId.Value && m.EndDate > DateTime.Now)
                .OrderByDescending(m => m.EndDate)
                .FirstOrDefaultAsync();

            if (existingMembership != null)
            {
                // Extend the existing one
                existingMembership.EndDate = existingMembership.EndDate.AddDays(plan.Duration);
                existingMembership.id_Membership = plan.id_Membership;
                _context.UserMemberships.Update(existingMembership);
            }
            else
            {
                // Create a new one
                var newMembership = new UserMembership
                {
                    id_User = userId.Value,
                    id_Membership = plan.id_Membership,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(plan.Duration),
                    Status = "Active"
                };
                _context.UserMemberships.Add(newMembership);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Bạn đã đăng ký thành công gói {plan.Name}. Bây giờ bạn có thể trải nghiệm tính năng Nghe Audio!";
            
            return RedirectToAction("Index");
        }
    }
}
