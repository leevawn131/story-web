using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using story_web.Data;
using story_web.Extensions;
using story_web.Filters;
using story_web.Models;

namespace story_web.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (HttpContext.Session.IsAuthenticated())
        {
            return RedirectAfterLogin(returnUrl, HttpContext.Session.GetCurrentUserRole() ?? UserRoles.NormalUser);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        model.UserName = NormalizeInput(model.UserName);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedUserName = NormalizeKey(model.UserName);
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.UserName.ToLower() == normalizedUserName);

        if (user is null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không hợp lệ.");
            return View(model);
        }

        HttpContext.Session.SetCurrentUser(user);
        TempData["SuccessMessage"] = $"Chào mừng quay lại, {user.UserName}.";

        return RedirectAfterLogin(model.ReturnUrl, user.Role);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.IsAuthenticated())
        {
            return RedirectAfterLogin(null, HttpContext.Session.GetCurrentUserRole() ?? UserRoles.NormalUser);
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        model.UserName = NormalizeInput(model.UserName);
        model.Email = NormalizeInput(model.Email);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedUserName = NormalizeKey(model.UserName);
        var normalizedEmail = NormalizeKey(model.Email);

        if (await _context.Users.AnyAsync(item => item.UserName.ToLower() == normalizedUserName))
        {
            ModelState.AddModelError(nameof(model.UserName), "Tên đăng nhập này đã được sử dụng.");
        }

        if (await _context.Users.AnyAsync(item => item.Email.ToLower() == normalizedEmail))
        {
            ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new User
        {
            UserName = model.UserName,
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Role = UserRoles.NormalUser,
            CreatedDate = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await AddNotificationAsync(user.id_User, "Tài khoản của bạn đã sẵn sàng. Hãy bắt đầu đọc truyện hoặc xuất bản của riêng bạn.");
        await _context.SaveChangesAsync();

        HttpContext.Session.SetCurrentUser(user);
        TempData["SuccessMessage"] = "Tài khoản của bạn đã được tạo thành công.";

        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedEmail = (model.Email ?? string.Empty).Trim().ToLower();

        _ = await _context.Users
            .AsNoTracking()
            .AnyAsync(item => item.Email.ToLower() == normalizedEmail);

        ViewBag.ResetMessage = "Nếu tài khoản tồn tại cho email đó, sẽ gửi liên kết đặt lại. Demo này chỉ mô phỏng yêu cầu.";
        ModelState.Clear();

        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.ClearCurrentUser();
        TempData["SuccessMessage"] = "Bạn đã đăng xuất.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [Auth]
    public async Task<IActionResult> Profile()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            HttpContext.Session.ClearCurrentUser();
            return RedirectToAction(nameof(Login));
        }

        return View(await BuildProfileViewModelAsync(user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Auth]
    public async Task<IActionResult> UpdateProfile(UpdateProfileInputModel model)
    {
        model.UserName = NormalizeInput(model.UserName);
        model.Email = NormalizeInput(model.Email);
        model.PenName = string.IsNullOrWhiteSpace(model.PenName) ? null : model.PenName.Trim();

        var user = await GetCurrentUserAsync(trackChanges: true);
        if (user is null)
        {
            HttpContext.Session.ClearCurrentUser();
            return RedirectToAction(nameof(Login));
        }

        var normalizedUserName = NormalizeKey(model.UserName);
        var normalizedEmail = NormalizeKey(model.Email);

        if (await _context.Users.AnyAsync(item => item.id_User != user.id_User && item.UserName.ToLower() == normalizedUserName))
        {
            ModelState.AddModelError(nameof(model.UserName), "Tên đăng nhập này đã được sử dụng.");
        }

        if (await _context.Users.AnyAsync(item => item.id_User != user.id_User && item.Email.ToLower() == normalizedEmail))
        {
            ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng.");
        }

        if (!ModelState.IsValid)
        {
            return View("Profile", await BuildProfileViewModelAsync(user, model));
        }

        user.UserName = model.UserName;
        user.Email = model.Email;
        user.ModifiedAt = DateTime.UtcNow;

        await AddNotificationAsync(user.id_User, "Thông tin hồ sơ của bạn đã được cập nhật.");
        await _context.SaveChangesAsync();

        HttpContext.Session.SetCurrentUser(user);
        TempData["SuccessMessage"] = "Hồ sơ của bạn đã được cập nhật.";

        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Auth]
    public async Task<IActionResult> ChangePassword(ChangePasswordInputModel model)
    {
        var user = await GetCurrentUserAsync(trackChanges: true);
        if (user is null)
        {
            HttpContext.Session.ClearCurrentUser();
            return RedirectToAction(nameof(Login));
        }

        if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash))
        {
            ModelState.AddModelError(nameof(model.OldPassword), "Mật khẩu cũ không chính xác.");
        }

        if (model.OldPassword == model.NewPassword)
        {
            ModelState.AddModelError(nameof(model.NewPassword), "Vui lòng chọn mật khẩu mới.");
        }

        if (!ModelState.IsValid)
        {
            var profileInput = new UpdateProfileInputModel
            {
                UserName = user.UserName,
                Email = user.Email,
                PenName = user.Author?.PenName
            };

            return View("Profile", await BuildProfileViewModelAsync(user, profileInput));
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        user.ModifiedAt = DateTime.UtcNow;

        await AddNotificationAsync(user.id_User, "Mật khẩu của bạn đã được thay đổi thành công.");
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Mật khẩu của bạn đã được thay đổi.";

        return RedirectToAction(nameof(Profile));
    }

    private async Task<User?> GetCurrentUserAsync(bool trackChanges = false)
    {
        var currentUserId = HttpContext.Session.GetCurrentUserId();
        if (!currentUserId.HasValue)
        {
            return null;
        }

        IQueryable<User> query = _context.Users
            .Include(item => item.Author);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(item => item.id_User == currentUserId.Value);
    }

    private async Task<ProfileViewModel> BuildProfileViewModelAsync(User user, UpdateProfileInputModel? profileInput = null)
    {
        var readingHistory = await _context.ReadingHistories
            .AsNoTracking()
            .Include(item => item.Story)
            .Include(item => item.Chapter)
            .Where(item => item.id_User == user.id_User)
            .OrderByDescending(item => item.Last_Read_At)
            .Take(8)
            .Select(item => new ProfileStoryItemViewModel
            {
                StoryId = item.id_Story ?? 0,
                ChapterId = item.id_Chapter,
                StoryName = item.Story != null ? (item.Story.StoryName ?? "Untitled story") : "Unknown story",
                PostStatus = item.Story != null ? item.Story.PostStatus : null,
                Subtitle = item.Chapter != null
                    ? $"{item.Chapter.ChapterNumber:0.##} - {item.Chapter.ChapterName}"
                    : "Recently read",
                ActivityAt = item.Last_Read_At
            })
            .ToListAsync();

        var favouriteStories = await _context.Favourites
            .AsNoTracking()
            .Include(item => item.Story)
            .Where(item => item.id_User == user.id_User)
            .OrderByDescending(item => item.Added_At)
            .Take(8)
            .Select(item => new ProfileStoryItemViewModel
            {
                StoryId = item.id_Story ?? 0,
                StoryName = item.Story != null ? (item.Story.StoryName ?? "Untitled story") : "Unknown story",
                PostStatus = item.Story != null ? item.Story.PostStatus : null,
                Subtitle = "Saved to favorites",
                ActivityAt = item.Added_At
            })
            .ToListAsync();

        var postedStories = await _context.Stories
            .AsNoTracking()
            .Include(item => item.Author)
            .Where(item => item.Author != null && item.Author.id_User == user.id_User)
            .OrderByDescending(item => item.Modified_At ?? item.Posted_At)
            .Take(8)
            .Select(item => new ProfileStoryItemViewModel
            {
                StoryId = item.id_Story,
                StoryName = item.StoryName ?? "Untitled story",
                PostStatus = item.PostStatus,
                Subtitle = item.Reject_Reason,
                ActivityAt = item.Modified_At ?? item.Posted_At
            })
            .ToListAsync();

        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(item => item.id_User == user.id_User)
            .OrderByDescending(item => item.Created_At)
            .Take(5)
            .Select(item => new NotificationItemViewModel
            {
                NotificationId = item.id_Noti,
                Content = item.Content ?? string.Empty,
                IsRead = item.IsRead ?? false,
                CreatedAt = item.Created_At
            })
            .ToListAsync();

        return new ProfileViewModel
        {
            UserId = user.id_User,
            UserName = user.UserName,
            Email = user.Email,
            PenName = user.Author?.PenName,
            CreatedDate = user.CreatedDate,
            UpdateProfile = profileInput ?? new UpdateProfileInputModel
            {
                UserName = user.UserName,
                Email = user.Email,
                PenName = user.Author?.PenName
            },
            ReadingHistory = readingHistory,
            FavouriteStories = favouriteStories,
            PostedStories = postedStories,
            Notifications = notifications
        };
    }

    private Task AddNotificationAsync(int userId, string content)
    {
        _context.Notifications.Add(new Notification
        {
            id_User = userId,
            Content = content,
            IsRead = false,
            Created_At = DateTime.UtcNow
        });

        return Task.CompletedTask;
    }

    private IActionResult RedirectAfterLogin(string? returnUrl, int role)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        if (role == UserRoles.Admin)
        {
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        return RedirectToAction(nameof(Profile));
    }

    private static string NormalizeInput(string? value)
    {
        return (value ?? string.Empty).Trim();
    }

    private static string NormalizeKey(string? value)
    {
        return NormalizeInput(value).ToLower();
    }
}
