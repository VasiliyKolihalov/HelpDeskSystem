using System.ComponentModel.DataAnnotations;
using HelpDeskSystem.Attributes;

namespace HelpDeskSystem.Models.User;

public class UserCreate
{
    [Required] [MaxLength(100)] public string FirstName { get; set; }
    [Required] [MaxLength(100)] public string LastName { get; set; }
    [EmailAddress] [MaxLength(100)] public string Email { get; set; }
    [Password] public string Password { get; set; }
}