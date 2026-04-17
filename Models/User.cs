using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace story_web.Models;

[Table("Users")]
public class User
{
    [Key]
    public int id_User { get; set; }

    [Required]
    [StringLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("Password")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public int Role { get; set; } = UserRoles.NormalUser;

    [Column("created_at")]
    public DateTime? CreatedDate { get; set; }

    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }

    public Author? Author { get; set; }

    public ICollection<Notification> Notifications { get; set; } = [];
}

public static class UserRoles
{
    public const int Admin = 1;
    public const int NormalUser = 2;
}
