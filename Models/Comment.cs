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

    public required string Content { get; set; }
    [ForeignKey("id_User")]  
    public  User? User { get; set; }
    [ForeignKey("id_Story")]
    public Story? Story { get; set; }

}
