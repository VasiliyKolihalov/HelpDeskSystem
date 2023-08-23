using System.Data.Common;
using Authentication.WebApi.Models.EmailConfirmCodes;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;

namespace Authentication.WebApi.Repositories.EmailConfirmCodes;

public class EmailConfirmCodesRepository : IEmailConfirmCodesRepository
{
    private readonly IDbContext _dbContext;

    public EmailConfirmCodesRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EmailConfirmCode?> GetByAccountIdAsync(Guid accountId)
    {
        const string query = "select * from AccountsEmailConfirmCodes where AccountId = @accountId";
        EmailConfirmCode? code = null;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            code = await transaction.QueryFirstOrDefaultAsync<EmailConfirmCode>(query, new { accountId });
        });
        return code;
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

    public async Task InsertAsync(EmailConfirmCode code)
    {
        const string query = "insert into AccountsEmailConfirmCodes values (@AccountId, @Code, @DateTime)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(
            async transaction => await transaction.ExecuteAsync(query, code));
    }

    public async Task DeleteByAccountIdAsync(Guid accountId)
    {
        const string query = "delete from AccountsEmailConfirmCodes where AccountId = @accountId";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(
            async transaction => await transaction.ExecuteAsync(query, new { accountId }));
    }
}