using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace story_web.Models;

[Table("Favourites")]
public class Favourite
{
    [Key]
    public int id_Favourite { get; set; }

    public int? id_Story { get; set; }

    public int? id_User { get; set; }

    public DateTime? Added_At { get; set; }

    [ForeignKey(nameof(id_Story))]
    public Story? Story { get; set; }

    [ForeignKey(nameof(id_User))]
    public User? User { get; set; }
}

[Table("Reading_History")]
public class ReadingHistory
{
    [Key]
    public int id_History { get; set; }

    public int? id_Story { get; set; }

    public int? id_User { get; set; }

    public int? id_Chapter { get; set; }

    public DateTime? Last_Read_At { get; set; }

    [ForeignKey(nameof(id_Story))]
    public Story? Story { get; set; }

    [ForeignKey(nameof(id_User))]
    public User? User { get; set; }

    [ForeignKey(nameof(id_Chapter))]
    public Chapter? Chapter { get; set; }
}

[Table("Notifications")]
public class Notification
{
    [Key]
    public int id_Noti { get; set; }

    public int? id_User { get; set; }

    public string? Content { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? Created_At { get; set; }

    [ForeignKey(nameof(id_User))]
    public User? User { get; set; }
}
