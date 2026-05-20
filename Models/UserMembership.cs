using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace story_web.Models;
[Table("UserMemberships")]
public class UserMembership
{
    [Key]
    public int id {get;set;}
    public int id_User {get;set;}
    public int id_Membership {get;set;}
    public DateTime StartDate {get;set;}
    public DateTime EndDate {get;set;}
    public string? Status {get;set;}
    [ForeignKey("id_User")]
    public User? User {get;set;}
    [ForeignKey("id_Membership")]
    public Membership? Membership {get;set;}
}