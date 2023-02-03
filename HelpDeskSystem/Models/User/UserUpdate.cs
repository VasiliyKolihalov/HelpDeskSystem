using System.ComponentModel.DataAnnotations;

namespace HelpDeskSystem.Models.User;

public class UserUpdate
{
    [Required] public Guid Id { get; set; }
    [Required] [MaxLength(100)] public string FirstName { get; set; }
    [Required] [MaxLength(100)] public string LastName { get; set; }
}