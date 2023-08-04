using System.ComponentModel.DataAnnotations;
using Authentication.Infrastructure.Attributes;

namespace Authentication.WebApi.Models.Accounts;

public class ChangePassword
{
    [Required] [Password] public string CurrentPassword { get; set; }
    [Required] [Password] public string NewPassword { get; set; }
}