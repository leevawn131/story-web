using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace story_web.Models;

[Table("ChapterAudio")]
public class ChapterAudio
{
    [Key]
    public int id_Audio { get; set; }

    public int? id_Chapter { get; set; }

    public string? AudioUrl { get; set; }

    public DateTime? Generated_At { get; set; }
}
