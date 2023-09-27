using System.ComponentModel.DataAnnotations;

namespace Authentication.Api.Models.Accounts;

public class ChangeEmail
{
    [Required] [MaxLength(150)] [EmailAddress] public string NewEmail { get; set; }
}