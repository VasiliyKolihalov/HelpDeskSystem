using Authentication.WebApi.Models.Permissions;

namespace Authentication.WebApi.Models.Roles;

public class UserRole
{
    public string Id { get; set; }
    public IEnumerable<UserPermission> Permissions { get; set; }
}