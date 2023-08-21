using System.Data.Common;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;

namespace Authentication.WebApi.Repositories.ConfirmCodes;

public class ConfirmCodesRepository : IConfirmCodesRepository
{
    private readonly IDbContext _dbContext;

    public ConfirmCodesRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string?> GetByAccountIdAsync(Guid accountId)
    {
        const string query = "select ConfirmCode from AccountsEmailConfirmCodes where AccountId = @accountId";
        string? confirmCode = null;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            confirmCode = await transaction.QueryFirstOrDefaultAsync<string>(query, new { accountId });
        });
        return confirmCode;
    }

    public async Task<bool> IsExistsAsync(Guid accountId)
    {
        const string query = "select exists(select * from AccountsEmailConfirmCodes where AccountId = @accountId)";
        var exists = false;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            exists = await transaction.QuerySingleAsync<bool>(query, new { accountId });
        });
        return exists;
    }

    public async Task InsertAsync(Guid accountId, string code)
    {
        const string query = "insert into AccountsEmailConfirmCodes values (@accountId, @code)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(
            async transaction => await transaction.ExecuteAsync(query, new { accountId, code }));
    }

    public async Task DeleteAsync(Guid accountId)
    {
        const string query = "delete from AccountsEmailConfirmCodes where AccountId = @accountId";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(
            async transaction => await transaction.ExecuteAsync(query, new { accountId }));
    }
}