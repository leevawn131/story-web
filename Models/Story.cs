using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
namespace story_web.Models;

[Table("Stories")]
public class Story
{
    [Key]
    public int id_Story { get; set; }

    public int? id_Author { get; set; }

    public string? StoryName { get; set; }


    public string? Description { get; set; }

    [StringLength(250)]
    public string? Image { get; set; }

    public DateTime? Modified_At { get; set; }

    public int? Views { get; set; }

    public double? Rating { get; set; }

    public string? PostStatus { get; set; }

    [StringLength(255)]
    public string? Reject_Reason { get; set; }

    public DateTime? Posted_At { get; set; }

    [ForeignKey(nameof(id_Author))]
    public Author? Author { get; set; }
    public string? StoryStatus { get; set; }
    public ICollection<StoryCategory> StoryCategories { get; set; }
    = new List<StoryCategory>();

    public ICollection<Chapter> Chapters { get; set; } = [];
    [Display(Name = "Nguồn truyện")]
    public bool IsOriginal { get; set; } = true;
    [Display(Name = "Tên tác giả gốc")]
    [StringLength(150, ErrorMessage = "Tên tác giả gốc không được vượt quá 150 ký tự")]
    public string? OriginalAuthor { get; set; }


}
