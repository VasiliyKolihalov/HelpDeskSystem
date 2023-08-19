using Authentication.WebApi.Models.Accounts;
using Infrastructure.Repositories;

namespace Authentication.WebApi.Repositories.Accounts;

public interface IAccountsRepository : IRepository<UserAccount, Guid>
{
    public Task<UserAccount?> GetByEmailAsync(string email);
    public Task<bool> IsExistsAsync(string email);
    public Task<string?> GetConfirmCodeByIdAsync(Guid accountId);
    public Task<bool> IsExistsConfirmCodeAsync(Guid accountId);
    public Task AddConfirmCodeAsync(Guid accountId, string code);
    public Task DeleteConfirmCodeAsync(Guid accountId);
    public Task AddRoleAsync(Guid accountId, string roleId);
    public Task RemoveRoleAsync(Guid accountId, string roleId);
}