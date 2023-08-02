using System.ComponentModel.DataAnnotations;

namespace Authentication.WebApi.Models.Accounts;

public class ChangeEmail
{
    [Required] [MaxLength(150)] [EmailAddress] public string NewEmail { get; set; }
}