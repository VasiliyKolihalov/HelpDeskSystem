using Authentication.WebApi.Models.Permissions;

namespace Authentication.WebApi.Models.Roles;

public class UserRoleView
{
    public string Id { get; set; }
    public IEnumerable<UserPermissionView> Permissions { get; set; }
}