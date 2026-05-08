using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace story_web.Models;

[Table("Categories")]
public class Category
{
    [Key]
    public int id_Category { get; set; }

    [StringLength(50)]
    public string? CategoryName { get; set; }

    public string? Description { get; set; }

public ICollection<StoryCategory> StoryCategories { get; set; }
    = new List<StoryCategory>();}
