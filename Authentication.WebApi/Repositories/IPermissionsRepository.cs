using Authentication.WebApi.Models;
using Authentication.WebApi.Models.Permissions;

namespace Authentication.WebApi.Repositories;

public interface IPermissionsRepository
{
    public Task<IEnumerable<UserPermission>> GetAllAsync();
    public Task<bool> IsExistsAsync(string id);
}