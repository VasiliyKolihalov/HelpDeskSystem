using Authentication.Api.Models.Permissions;

namespace Authentication.Api.Repositories.Permissions;

public interface IPermissionsRepository
{
    public Task<IEnumerable<UserPermission>> GetAllAsync();
    public Task<bool> IsExistsAsync(string id);
}