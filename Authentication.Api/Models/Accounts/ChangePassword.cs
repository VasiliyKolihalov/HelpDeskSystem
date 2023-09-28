using System.ComponentModel.DataAnnotations;
using Infrastructure.Authentication.Attributes;

namespace Authentication.Api.Models.Accounts;

public class ChangePassword
{
    [Required] [Password] public string CurrentPassword { get; set; }
    [Required] [Password] public string NewPassword { get; set; }
}