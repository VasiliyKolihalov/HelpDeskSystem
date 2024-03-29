﻿using System.Data.Common;
using Authentication.Api.Models.Accounts;
using Authentication.Api.Models.Permissions;
using Authentication.Api.Models.Roles;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;

namespace Authentication.Api.Repositories.Accounts;

public class AccountsRepository : IAccountsRepository
{
    private readonly IDbContext _dbContext;

    public AccountsRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<UserAccount>> GetAllAsync()
    {
        const string accountQuery = "select * from Accounts";

        const string rolesQuery = "select * from Roles " +
                                  "roles inner join AccountsRoles accountsRoles on roles.Id = accountsRoles.RoleId " +
                                  "where accountsRoles.AccountId = @Id";

        const string permissionsQuery = "select * from Permissions " +
                                        " inner join RolesPermissions rolesPermissions on Permissions.Id = rolesPermissions.PermissionId " +
                                        "where rolesPermissions.RoleId = @Id";
        IEnumerable<UserAccount> accounts = null!;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            accounts = (await transaction.QueryAsync<UserAccount>(accountQuery)).ToList();
            
            foreach (UserAccount account in accounts)
            {
                IEnumerable<UserRole> roles =
                    (await transaction.QueryAsync<UserRole>(rolesQuery, param: new { account.Id })).ToList();

                foreach (UserRole role in roles)
                {
                    role.Permissions = await transaction.QueryAsync<UserPermission>(
                        sql: permissionsQuery,
                        param: new { role.Id });
                }

                account.Roles = roles;
            }
        });
        return accounts;
    }

    public async Task<UserAccount?> GetByIdAsync(Guid id)
    {
        const string accountQuery = "select * from Accounts where Id = @id";
        const string rolesQuery = "select * from Roles roles " +
                                  "inner join AccountsRoles accountsRoles on roles.Id = accountsRoles.RoleId " +
                                  "where accountsRoles.AccountId = @id";
        const string permissionsQuery = "select * from Permissions " +
                                        "inner join RolesPermissions rolesPermissions on Permissions.Id = rolesPermissions.PermissionId " +
                                        "where rolesPermissions.RoleId = @Id";
        UserAccount? account = null;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            account = await transaction.QueryFirstOrDefaultAsync<UserAccount>(accountQuery, param: new { id });
            if (account == null)
                return;

            IEnumerable<UserRole> roles =
                (await transaction.QueryAsync<UserRole>(rolesQuery, param: new { account.Id })).ToList();

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

    public async Task<bool> IsExistsAsync(Guid id)
    {
        const string query = "select exists(select * from Accounts where Id = @id)";
        var exists = false;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            exists = await transaction.QuerySingleAsync<bool>(query, new { id });
        });
        return exists;
    }

    public async Task InsertAsync(UserAccount userAccount)
    {
        const string query = "insert into Accounts values (@Id, @Email, @PasswordHash, @IsEmailConfirm)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(
            async transaction => await transaction.ExecuteAsync(query, userAccount));
    }

    public async Task UpdateAsync(UserAccount item)
    {
        const string query = "update Accounts set " +
                             "Email = @Email, " +
                             "IsEmailConfirm = @IsEmailConfirm, " +
                             "PasswordHash = @PasswordHash " +
                             "where Id = @Id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(
            async transaction => await transaction.ExecuteAsync(query, item));
    }

    public async Task DeleteAsync(Guid id)
    {
        const string query = "delete from Accounts where Id = @id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(
            async transaction => await transaction.ExecuteAsync(query, new { id }));
    }

    public async Task<UserAccount?> GetByEmailAsync(string email)
    {
        const string accountQuery = "select * from Accounts where Email = @email";
        const string rolesQuery = "select * from Roles roles " +
                                  "inner join AccountsRoles accountsRoles on roles.id = accountsRoles.RoleId " +
                                  "where accountsRoles.AccountId = @Id";
        const string permissionsQuery = "select * from Permissions " +
                                        "inner join RolesPermissions rolesPermissions on Permissions.Id = rolesPermissions.PermissionId " +
                                        "where rolesPermissions.RoleId = @Id";
        UserAccount? account = null;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            account = await transaction.QueryFirstOrDefaultAsync<UserAccount>(accountQuery, param: new { email });
            if (account == null)
                return;

            IEnumerable<UserRole> roles =
                (await transaction.QueryAsync<UserRole>(rolesQuery, param: new { account.Id })).ToList();

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
        var exists = false;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            exists = await transaction.QuerySingleAsync<bool>(query, new { email });
        });
        return exists;
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