using System.ComponentModel.DataAnnotations;

namespace Users.Api.Models.Users;

public class UserUpdate
{
    [Required] public Guid Id { get; set; }
    [Required] [MaxLength(150)] public string FirstName { get; set; }
    [Required] [MaxLength(150)] public string LastName { get; set; }
}