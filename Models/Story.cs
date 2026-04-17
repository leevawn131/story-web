using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace story_web.Models;

[Table("Stories")]
public class Story
{
    [Key]
    public int id_Story { get; set; }

    public int? id_Author { get; set; }

    public string? StoryName { get; set; }

    public int? id_Category { get; set; }

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

    [ForeignKey(nameof(id_Category))]
    public Category? Category { get; set; }

    public ICollection<Chapter> Chapters { get; set; } = [];
}
