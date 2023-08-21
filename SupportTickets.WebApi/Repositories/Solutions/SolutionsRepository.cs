using System.Data.Common;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;
using SupportTickets.WebApi.Models.Solutions;

namespace SupportTickets.WebApi.Repositories.Solutions;

public class SolutionsRepository : ISolutionsRepository
{
    private readonly IDbContext _dbContext;

    public SolutionsRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InsertAsync(Solution solution)
    {
        const string query = "insert into Solutions values (@MessageId, @Status)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            var param = new { solution.MessageId, Status = solution.Status.ToString() };
            await transaction.ExecuteAsync(query, param);
        });
    }

    public async Task UpdateAsync(Solution solution)
    {
        const string query = "update Solutions set Status = @Status where MessageId = @MessageId";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, new { solution.MessageId, Status = solution.Status.ToString() });
        });
    }
}