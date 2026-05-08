using System.ComponentModel.DataAnnotations.Schema;

namespace story_web.Models;

[Table("StoryCategories")]
public class StoryCategory
{
    public int id_Story { get; set; }

    public int id_Category { get; set; }

    public virtual Story? Story { get; set; }

    public virtual Category? Category { get; set; }
}