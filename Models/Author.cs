using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace story_web.Models
{
    public class Author
    {
        [Key]
        public int id_Author {get;set;}
        public string? PenName {get;set;}
    }
}