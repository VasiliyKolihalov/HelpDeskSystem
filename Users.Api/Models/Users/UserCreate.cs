using System.ComponentModel.DataAnnotations;

namespace Users.Api.Models.Users;

public class UserCreate
{
    [Required] [MaxLength(150)] public string FirstName { get; set; }
    [Required] [MaxLength(150)] public string LastName { get; set; }
    [Required] [MaxLength(150)] [EmailAddress] public string Email { get; set; }
}