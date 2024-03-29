﻿using System.ComponentModel.DataAnnotations;

namespace Authentication.Api.Models.Accounts;

public class UserAccountLogin
{
    [Required] public string Email { get; set; }
    [Required] public string Password { get; set; }
}