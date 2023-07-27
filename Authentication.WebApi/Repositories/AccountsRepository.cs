using System.Data.Common;
using Authentication.WebApi.Models;
using Authentication.WebApi.Models.Accounts;
using Authentication.WebApi.Models.Permissions;
using Authentication.WebApi.Models.Roles;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;

namespace Authentication.WebApi.Repositories;

public class AccountsRepository : IAccountsRepository
{
    private readonly IDbContext _dbContext;

    public AccountsRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserAccount?> GetByIdAsync(Guid id)
    {
        const string accountQuery = @"select * from Accounts where Id = @id";
        const string rolesQuery = @"select * from Roles roles
                                    inner join AccountsRoles accountsRoles on roles.Id = accountsRoles.RoleId
                                    where accountsRoles.AccountId = @id";
        const string permissionsQuery = @"select * from Permissions
                                          inner join RolesPermissions rolesPermissions on Permissions.Id = rolesPermissions.PermissionId
                                          where rolesPermissions.RoleId = @Id";
        UserAccount? account = null;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            account = await transaction.QueryFirstOrDefaultAsync<UserAccount>(accountQuery, param: new { id });
            if (account == null)
                return;

            IEnumerable<UserRole> roles =
                (await transaction.QueryAsync<UserRole>(rolesQuery, param: new { account.Id })).ToList();

            if (!roles.Any())
            {
                account.Roles = Array.Empty<UserRole>();
                return;
            }

            foreach (UserRole role in roles)
            {
                role.Permissions = await transaction.QueryAsync<UserPermission>(
                    sql: permissionsQuery,
                    param: new { role.Id });
            }

            account.Roles = roles;
        });
        return account;
    }

    public async Task<UserAccount?> GetByEmailAsync(string email)
    {
        const string accountQuery = @"select * from Accounts where Email = @email";
        const string rolesQuery = @"select * from Roles roles
                                    inner join AccountsRoles accountsRoles on roles.id = accountsRoles.RoleId
                                    where accountsRoles.AccountId = @Id";
        const string permissionsQuery = @"select * from Permissions
                                          inner join RolesPermissions rolesPermissions on Permissions.Id = rolesPermissions.PermissionId
                                          where rolesPermissions.RoleId = @Id";
        UserAccount? account = null;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            account = await transaction.QueryFirstOrDefaultAsync<UserAccount>(accountQuery, param: new { email });
            if (account == null)
                return;

            IEnumerable<UserRole> roles =
                (await transaction.QueryAsync<UserRole>(rolesQuery, param: new { account.Id })).ToList();

            if (!roles.Any())
            {
                account.Roles = Array.Empty<UserRole>();
                return;
            }

            foreach (UserRole role in roles)
            {
                role.Permissions = await transaction.QueryAsync<UserPermission>(
                    sql: permissionsQuery,
                    param: new { role.Id });
            }

            account.Roles = roles;
        });
        return account;
    }

    public async Task<bool> IsExistsAsync(string email)
    {
        const string query = "select exists(select * from Accounts where Email = @email)";
        await using DbConnection connection = _dbContext.CreateConnection();
        var exists = false;
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            exists = await transaction.QuerySingleAsync<bool>(query, new { email });
        });
        return exists;
    }

    public async Task InsertAsync(UserAccount userAccount)
    {
        const string query = "insert into Accounts values (@Id, @Email, @PasswordHash)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(
            async transaction => await transaction.ExecuteAsync(query, userAccount));
    }

    public async Task AddRoleAsync(Guid accountId, string roleId)
    {
        const string query = "insert into AccountsRoles values (@accountId, @roleId)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(
            async transaction => await transaction.ExecuteAsync(query, new { accountId, roleId }));
    }

    public async Task RemoveRoleAsync(Guid accountId, string roleId)
    {
        const string query = "delete from AccountsRoles where AccountId = @accountId and RoleId = @roleId";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(
            async transaction => await transaction.ExecuteAsync(query, new { accountId, roleId }));
    }
}