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

        [Auth]
        public async Task<IActionResult> Checkout(int id)
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

            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Auth]
        public async Task<IActionResult> ProcessPayment(int id, string paymentMethod, string status)
        {
            var userId = HttpContext.Session.GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var plan = await _context.Memberships.FindAsync(id);
            if (plan == null)
            {
                return NotFound();
            }

            // Tạo bản ghi Payment
            var payment = new Payment
            {
                id_User = userId.Value,
                id_Membership = plan.id_Membership,
                Amount = plan.Price,
                PaymentMethod = paymentMethod,
                Status = status, // "success" hoặc "failed"
                Created_At = DateTime.Now
            };
            _context.Payments.Add(payment);

            if (status == "success")
            {
                // Kiểm tra xem user đã có gói hội viên còn hạn hay không
                var existingMembership = await _context.UserMemberships
                    .Where(m => m.id_User == userId.Value && m.EndDate > DateTime.Now)
                    .OrderByDescending(m => m.EndDate)
                    .FirstOrDefaultAsync();

                if (existingMembership != null)
                {
                    // Cộng dồn hạn dùng
                    existingMembership.EndDate = existingMembership.EndDate.AddDays(plan.Duration);
                    existingMembership.id_Membership = plan.id_Membership;
                    _context.UserMemberships.Update(existingMembership);
                }
                else
                {
                    // Tạo đăng ký mới
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
                TempData["SuccessMessage"] = $"Thanh toán thành công! Bạn đã đăng ký/gia hạn thành công gói {plan.Name} ({plan.Duration} ngày).";
            }
            else
            {
                await _context.SaveChangesAsync();
                TempData["ErrorMessage"] = $"Thanh toán thất bại! Giao dịch mua gói {plan.Name} không thành công.";
            }

            return RedirectToAction("Index");
        }
    }
}
