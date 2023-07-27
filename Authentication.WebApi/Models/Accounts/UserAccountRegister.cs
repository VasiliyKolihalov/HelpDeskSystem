using System.ComponentModel.DataAnnotations;

namespace Authentication.WebApi.Models.Accounts;

public class UserAccountRegister
{
    [Required] [MaxLength(150)] public string FirstName { get; set; }
    [Required] [MaxLength(150)] public string LastName { get; set; }
    [Required] [MaxLength(150)] public string Email { get; set; }
    [Required] public string Password { get; set; }
}