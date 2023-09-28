using Authentication.Api.Models.Roles;
using Infrastructure.Repositories;

namespace Authentication.Api.Repositories.Roles;

public interface IRolesRepository : IRepository<UserRole, string>
{
    public Task AddPermissionAsync(string roleId, string permissionId);
    public Task RemoveAllPermissionsAsync(string roleId);
}