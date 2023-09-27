using System.Data.Common;
using Authentication.Api.Models.Permissions;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;

namespace Authentication.Api.Repositories.Permissions;

public class PermissionsRepository : IPermissionsRepository
{
    private readonly IDbContext _dbContext;

    public PermissionsRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<UserPermission>> GetAllAsync()
    {
        const string query = "select * from Permissions";
        IEnumerable<UserPermission> permissions = null!;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            permissions = await transaction.QueryAsync<UserPermission>(query);
        });
        return permissions;
    }

    public async Task<bool> IsExistsAsync(string id)
    {
        const string query = "select exists(select * from Permissions where Id = @id)";
        await using DbConnection connection = _dbContext.CreateConnection();
        var exists = false;
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            exists = await transaction.QuerySingleAsync<bool>(query, new { id });
        });
        return exists;
    }
}