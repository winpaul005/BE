using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BE.Models;
//all the stuff for login n stuff
public class LoginModel
{
    public string username {get; set;} = "";
    public string passwd {get; set;} = "";
}

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsUsed { get; set; }
}
[Table("users")]
public class User
{
    [Key]
    public int id {get;set;}
    public string username {get;set;}
    [Column(TypeName = "varchar")]
    [StringLength(60)]
    public string passwdhash {get;set;}
    public int role {get;set;}
    public string status {get;set;}
}
public class UserStatus
{
    public int completedTasks {get;set;}
}