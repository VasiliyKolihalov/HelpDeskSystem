using Authentication.WebApi.Models.Roles;
using Infrastructure.Repositories;

namespace Authentication.WebApi.Repositories;

public interface IRolesRepository : IRepository<UserRole, string>
{
    public Task AddPermissionAsync(string roleId, string permissionId);
    public Task RemoveAllPermissionsAsync(string roleId);
}