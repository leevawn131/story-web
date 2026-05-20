using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace story_web.Models;
[Table("Memberships")]
public class Membership
{
    [Key]
    public int id_Membership {get;set;}
    public string? Name {get;set;}
    public decimal Price {get;set;}
    public int Duration {get;set;}
    public string? Description {get;set;}
}