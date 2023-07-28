using Authentication.WebApi.Models.Accounts;

namespace Authentication.WebApi.Repositories;

public interface IAccountsRepository
{
    public Task<UserAccount?> GetByIdAsync(Guid id);
    public Task<UserAccount?> GetByEmailAsync(string email);
    public Task<bool> IsExistsAsync(string email);
    public Task InsertAsync(UserAccount account);
    public Task AddRoleAsync(Guid accountId, string roleId);
    public Task RemoveRoleAsync(Guid accountId, string roleId);
}