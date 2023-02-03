using System.Data.SqlClient;
using Dapper.Transaction;
using HelpDeskSystem.Exceptions;
using HelpDeskSystem.Extensions;
using HelpDeskSystem.Models.Roles;

namespace HelpDeskSystem.Repositories;

public class RolesRepository : IRolesRepository
{
    private readonly string _connectionString;

    public RolesRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        IEnumerable<Role> roles = null!;
        const string rolesQuery = "select * from Roles";
        const string permissionsQuery = "select Name from Permissions where RoleId = @Id";
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            roles = await transaction.QueryAsync<Role>(rolesQuery);

            foreach (Role role in roles)
            {
                role.Permissions = await transaction.QueryAsync<string>(permissionsQuery, new { role.Id });
            }
        });

        return roles;
    }

    public async Task<Role> GetByIdAsync(Guid id)
    {
        await using var connection = new SqlConnection(_connectionString);
        Role role = null!;
        const string roleQuery = @"select * from Roles where Id = @id";
        const string permissionsQuery = "select Name from Permissions where RoleId = @id";
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            role = await transaction.QuerySingleOrDefaultAsync<Role>(roleQuery, new { id });

            if (role == null)
                throw new NotFoundException("Role not found");

            role.Permissions = await transaction.QueryAsync<string>(permissionsQuery, new { id });
        });

        return role;
    }

    public async Task<IEnumerable<Role>> GetByUserIdAsync(Guid userId)
    {
        await using var connection = new SqlConnection(_connectionString);
        IEnumerable<Role> roles = null!;
        const string rolesQuery = @"select * from Roles
                                    inner join UsersRoles on Roles.Id = UsersRoles.RoleId
                                    where UsersRoles.UserId = @userId";
        const string permissionsQuery = "select Name from Permissions where RoleId = @Id";
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            roles = await transaction.QueryAsync<Role>(rolesQuery, new { userId });

            foreach (Role role in roles)
            {
                role.Permissions = await transaction.QueryAsync<string>(permissionsQuery, new { role.Id });
            }
        });

        return roles;
    }

    public async Task AddToUser(string roleName, Guid userId)
    {
        await using var connection = new SqlConnection(_connectionString);
        const string query = "insert into UsersRoles values (@userId, (select Id from Roles where Name = @roleName))";
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, new { userId, roleName });
        });
    }
}