using HelpDeskSystem.Models.Roles;

namespace HelpDeskSystem.Repositories;

public interface IRolesRepository
{
    public Task<IEnumerable<Role>> GetAllAsync();
    public Task<Role> GetByIdAsync(Guid id);
    public Task<IEnumerable<Role>> GetByUserIdAsync(Guid userId);
    public Task AddToUser(string roleName, Guid userId);
}