using System.ComponentModel.DataAnnotations;

namespace story_web.Models;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required]
    [Display(Name = "Username")]
    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(6)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}

public class UpdateProfileInputModel
{
    [Required]
    [Display(Name = "Username")]
    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Pen name")]
    [StringLength(100)]
    public string? PenName { get; set; }
}

public class ChangePasswordInputModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Old password")]
    public string OldPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    [Compare(nameof(NewPassword), ErrorMessage = "New passwords do not match.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class ProfileStoryItemViewModel
{
    public int StoryId { get; set; }

    public int? ChapterId { get; set; }

    public string StoryName { get; set; } = string.Empty;

    public string? PostStatus { get; set; }

    public string? Subtitle { get; set; }

    public DateTime? ActivityAt { get; set; }
}

public class NotificationItemViewModel
{
    public int NotificationId { get; set; }

    public string Content { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }
}

public class ProfileViewModel
{
    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PenName { get; set; }

    public DateTime? CreatedDate { get; set; }

    public UpdateProfileInputModel UpdateProfile { get; set; } = new();

    public ChangePasswordInputModel ChangePassword { get; set; } = new();

    public IReadOnlyList<ProfileStoryItemViewModel> ReadingHistory { get; set; } = Array.Empty<ProfileStoryItemViewModel>();

    public IReadOnlyList<ProfileStoryItemViewModel> FavouriteStories { get; set; } = Array.Empty<ProfileStoryItemViewModel>();

    public IReadOnlyList<ProfileStoryItemViewModel> PostedStories { get; set; } = Array.Empty<ProfileStoryItemViewModel>();

    public IReadOnlyList<NotificationItemViewModel> Notifications { get; set; } = Array.Empty<NotificationItemViewModel>();

    public string? SupplementalDataNotice { get; set; }
}
