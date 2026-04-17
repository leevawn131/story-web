using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace story_web.Models;

[Table("Authors")]
public class Author
{
    [Key]
    public int id_Author { get; set; }

    public int? id_User { get; set; }

    [StringLength(100)]
    public string? PenName { get; set; }

    public string? Bio { get; set; }

    [StringLength(255)]
    public string? Avatar { get; set; }

    [ForeignKey(nameof(id_User))]
    public User? User { get; set; }

    public ICollection<Story> Stories { get; set; } = [];
}
