namespace Authentication.Infrastructure.Models;

public class Account<TId> where TId : notnull
{
    public TId Id { get; set; }
    public bool IsEmailConfirm { get; set; }
    public IEnumerable<Role>? Roles { get; set; }

    public IEnumerable<Permission>? Permissions;
    

    public bool HasRole(string roleId)
    {
        return Roles?.Any(_ => _.Id == roleId) ?? false;
    }

    public bool HasPermission(string permissionId)
    {
        return Permissions?.Any(_ => _.Id == permissionId) ?? false;
    }
}