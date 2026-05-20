using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace story_web.Models;
[Table("Payments")]
public class Payment
{
    [Key]
    public int id_Payment {get;set;}
    public int id_User {get;set;}
    public int id_Membership {get;set;}
    public decimal Amount {get;set;}
    public string? PaymentMethod {get;set;}
    public string? Status {get;set;}
    public DateTime Created_At {get;set;}
    [ForeignKey("id_User")]
    public User? User {get;set;}
    [ForeignKey("id_Membership")]
    public Membership? Membership {get;set;}
}