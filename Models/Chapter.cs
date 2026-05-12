using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace story_web.Models;

[Table("Chapters")]
public class Chapter
{
    [Key]
    public int id_Chapter { get; set; }

    public int? id_Story { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? ChapterNumber { get; set; }

    [StringLength(250)]
    public string? ChapterName { get; set; }
    public string? AISummary {get;set;}
    public string? AudioPath {get;set;}
    public DateTime? Posted_At { get; set; }

    public DateTime? Modified_At { get; set; }

    public string? Content { get; set; }

    [ForeignKey(nameof(id_Story))]
    public Story? Story { get; set; }
}
