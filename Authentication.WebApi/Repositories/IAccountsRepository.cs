using Authentication.WebApi.Models.Accounts;
using Infrastructure.Repositories;

namespace Authentication.WebApi.Repositories;

public interface IAccountsRepository : IRepository<UserAccount, Guid>
{
    public Task<UserAccount?> GetByEmailAsync(string email);
    public Task<bool> IsExistsAsync(string email);
    public Task AddRoleAsync(Guid accountId, string roleId);
    public Task RemoveRoleAsync(Guid accountId, string roleId);
}