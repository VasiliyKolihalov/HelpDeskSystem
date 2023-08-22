using System.Data.Common;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;
using SupportTickets.WebApi.Models.SupportTicketAgentRecords;

namespace SupportTickets.WebApi.Repositories.SupportTicketAgentRecords;

public class SupportTicketAgentRecordsRepository : ISupportTicketAgentRecordsRepository
{
    private readonly IDbContext _dbContext;

    public SupportTicketAgentRecordsRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<SupportTicketAgentRecord>> GetBySupportTicketIdAsync(Guid supportTicketId)
    {
        const string query = "select * from SupportTicketAgentRecords " +
                             "where SupportTicketId = @supportTicketId";

        IEnumerable<SupportTicketAgentRecord> records = null!;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            records = await transaction.QueryAsync<SupportTicketAgentRecord>(query, param: new { supportTicketId });
        });
        return records;
    }

    public async Task InsertAsync(SupportTicketAgentRecord record)
    {
        const string query = "insert into SupportTicketAgentRecords values (@SupportTicketId, @AgentId, @DateTime)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, record);
        });
    }
}