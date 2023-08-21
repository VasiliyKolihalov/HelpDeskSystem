using System.Data.Common;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;
using SupportTickets.WebApi.Models.SupportTicketStatusRecords;

namespace SupportTickets.WebApi.Repositories.SupportTicketStatusRecords;

public class SupportTicketStatusRecordsRepository : ISupportTicketStatusRecordsRepository
{
    private readonly IDbContext _dbContext;

    public SupportTicketStatusRecordsRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    
    public async Task<IEnumerable<SupportTicketStatusRecord>> GetBySupportTicketIdAsync(Guid supportTicketId)
    {
        const string query = "select * from SupportTicketStatusRecords " +
                             "where SupportTicketId = @supportTicketId";

        IEnumerable<SupportTicketStatusRecord> records = null!;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            records = await transaction.QueryAsync<SupportTicketStatusRecord>(query, param: new { supportTicketId });
        });
        return records;
    }

    public async Task InsertAsync(SupportTicketStatusRecord record)
    {
        const string query = "insert into SupportTicketStatusRecords values (@SupportTicketId, @Status, @DateTime)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, record);
        });
    }
}