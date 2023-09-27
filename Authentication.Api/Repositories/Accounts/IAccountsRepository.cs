using Authentication.Api.Models.Accounts;
using Infrastructure.Repositories;

namespace Authentication.Api.Repositories.Accounts;

public interface IAccountsRepository : IRepository<UserAccount, Guid>
{
    public Task<UserAccount?> GetByEmailAsync(string email);
    public Task<bool> IsExistsAsync(string email);
    public Task AddRoleAsync(Guid accountId, string roleId);
    public Task RemoveRoleAsync(Guid accountId, string roleId);
}