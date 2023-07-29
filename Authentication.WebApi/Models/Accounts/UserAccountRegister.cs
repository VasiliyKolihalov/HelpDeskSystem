using System.ComponentModel.DataAnnotations;
using Authentication.Infrastructure.Attributes;

namespace Authentication.WebApi.Models.Accounts;

public class UserAccountRegister
{
    [Required] [MaxLength(150)] public string FirstName { get; set; }
    [Required] [MaxLength(150)] public string LastName { get; set; }
    [Required] [MaxLength(150)] [EmailAddress] public string Email { get; set; }
    [Required] [Password] public string Password { get; set; }
}