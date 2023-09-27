using System.ComponentModel.DataAnnotations;

namespace Authentication.Api.Models.Roles;

public class UserRoleUpdate
{
    [Required] public string Id { get; set; }
    public IEnumerable<string> PermissionIds { get; set; }
}