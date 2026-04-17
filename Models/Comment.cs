using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace story_web.Models;

[Table("Comments")]
public class Comment
{
    [Key]
    public int id_Comment { get; set; }

    public int? id_Story { get; set; }

    public int? id_User { get; set; }

    public DateTime? Posted_At { get; set; }

    public string? Content { get; set; }
}
