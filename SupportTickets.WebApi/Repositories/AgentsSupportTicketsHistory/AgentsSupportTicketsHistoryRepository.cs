using System.Data.Common;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;
using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Repositories.AgentsSupportTicketsHistory;

public class AgentsSupportTicketsHistoryRepository : IAgentsSupportTicketsHistoryRepository
{
    private readonly IDbContext _dbContext;

    public AgentsSupportTicketsHistoryRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<User>> GetBySupportTicketIdAsync(Guid supportTicketId)
    {
        const string query = "select * from Users " +
                             "inner join SupportTicketsAgents agents on Users.Id = agents.AgentId " +
                             "where agents.SupportTicketId = @supportTicketId";

        IEnumerable<User> users = null!;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            users = await transaction.QueryAsync<User>(query, param: new { supportTicketId });
        });
        return users;
    }

    public async Task InsertAsync(Guid supportTicketId, Guid userId)
    {
        const string query = "insert into SupportTicketsAgents values (@supportTicketId, @userId)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.QueryAsync<User>(query, param: new { supportTicketId, userId });
        });
    }
}