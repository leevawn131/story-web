using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace story_web.Models
{
    public class Story
    {
        [Key]
        public int id_Story {get;set;}
        public string? StoryName { get; set; }
        public string? PostStatus { get; set; }

        public int id_Author { get; set; }
        [ForeignKey("id_Author")]
        public Author? Author { get; set; }
    }
}