﻿using System.ComponentModel.DataAnnotations;

namespace Authentication.Api.Models.Roles;

public class UserRoleCreate
{
    [Required] public string Id { get; set; }
    public IEnumerable<string> PermissionIds { get; set; }
}