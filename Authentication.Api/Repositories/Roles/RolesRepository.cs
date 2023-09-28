using System.Data.Common;
using Authentication.Api.Models.Permissions;
using Authentication.Api.Models.Roles;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;

namespace Authentication.Api.Repositories.Roles;

public class RolesRepository : IRolesRepository
{
    private readonly IDbContext _dbContext;

    public RolesRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<UserRole>> GetAllAsync()
    {
        const string rolesQuery = "select * from Roles";
        const string permissionsQuery = "select * from Permissions permissions " +
                                        "inner join RolesPermissions rolesPermissions on permissions.Id = rolesPermissions.PermissionId " +
                                        "where rolesPermissions.RoleId = @Id";

        IEnumerable<UserRole> roles = null!;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            roles = await transaction.QueryAsync<UserRole>(rolesQuery);
            foreach (UserRole role in roles)
            {
                role.Permissions = await transaction.QueryAsync<UserPermission>(permissionsQuery, new { role.Id });
            }
        });
        return roles;
    }

    public async Task<UserRole?> GetByIdAsync(string id)
    {
        const string roleQuery = "select * from Roles where Id = @id";
        const string permissionsQuery = "select * from Permissions permissions " +
                                        "inner join RolesPermissions rolesPermissions on permissions.Id = rolesPermissions.PermissionId " +
                                        "where rolesPermissions.RoleId = @Id";

        UserRole? role = null;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            role = await transaction.QueryFirstOrDefaultAsync<UserRole>(roleQuery, new { id });
            if (role == null)
                return;

            role.Permissions = await transaction.QueryAsync<UserPermission>(permissionsQuery, new { role.Id });
        });
        return role;
    }

    public async Task<bool> IsExistsAsync(string id)
    {
        const string query = "select exists(select * from Roles where Id = @id)";
        await using DbConnection connection = _dbContext.CreateConnection();
        var exists = false;
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            exists = await transaction.QuerySingleAsync<bool>(query, new { id });
        });
        return exists;
    }


    public async Task InsertAsync(UserRole item)
    {
        const string query = "insert into Roles values(@Id)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction => { await transaction.ExecuteAsync(query, item); });
    }

    public async Task UpdateAsync(UserRole item)
    {
        // no action required
    }

    public async Task DeleteAsync(string id)
    {
        const string query = "delete from Roles where Id = @id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, new { id });
        });
    }

    public async Task AddPermissionAsync(string roleId, string permissionId)
    {
        const string query = "insert into RolesPermissions values(@roleId, @permissionId)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, new { roleId, permissionId });
        });
    }

    public async Task RemoveAllPermissionsAsync(string roleId)
    {
        const string query = "delete from RolesPermissions where RoleId = @roleId";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, new { roleId });
        });
    }
}