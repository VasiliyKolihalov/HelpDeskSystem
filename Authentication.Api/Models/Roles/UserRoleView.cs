using Authentication.Api.Models.Permissions;

namespace Authentication.Api.Models.Roles;

public class UserRoleView
{
    public string Id { get; set; }
    public IEnumerable<UserPermissionView> Permissions { get; set; }
}