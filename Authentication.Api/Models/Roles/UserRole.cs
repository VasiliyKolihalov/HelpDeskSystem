using Authentication.Api.Models.Permissions;

namespace Authentication.Api.Models.Roles;

public class UserRole
{
    public string Id { get; set; }
    public IEnumerable<UserPermission> Permissions { get; set; }
}