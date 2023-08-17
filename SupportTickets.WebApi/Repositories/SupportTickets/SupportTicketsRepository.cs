using System.Data.Common;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;
using SupportTickets.WebApi.Models.Messages;
using SupportTickets.WebApi.Models.Solutions;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Repositories.SupportTickets;

public class SupportTicketsRepository : ISupportTicketsRepository
{
    private readonly IDbContext _dbContext;

    public SupportTicketsRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<SupportTicket>> GetAllAsync()
    {
        const string supportTicketsQuery = @"select * from SupportTickets
                                             left join Users us on SupportTickets.UserId = us.Id
                                             inner join Users agents on SupportTickets.AgentId = agents.Id";
        const string messagesQuery = @"select * from Messages 
                                       inner join Users users on Messages.UserId = users.Id
                                       where SupportTicketId = @Id";

        const string solutionsQuery = @"select * from Solutions
                                        inner join Messages messages on Solutions.MessageId = messages.Id
                                        where messages.SupportTicketId = @Id";

        IEnumerable<SupportTicket> tickets = null!;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            tickets = (await transaction.QueryAsync<SupportTicket, User, User, SupportTicket>(
                sql: supportTicketsQuery,
                map: MapSupportTicketQuery)).ToList();

            if (!tickets.Any())
                return;

            foreach (SupportTicket supportTicket in tickets)
            {
                supportTicket.Messages =
                    await transaction.QueryAsync<Message, User, Message>(messagesQuery, MapMessagesQuery,
                        new { supportTicket.Id });

                supportTicket.Solutions =
                    await transaction.QueryAsync<Solution>(solutionsQuery, new { supportTicket.Id });
            }
        });
        return tickets;
    }

    public async Task<SupportTicket?> GetByIdAsync(Guid id)
    {
        const string query = @"select * from SupportTickets
                               inner join Users us on SupportTickets.UserId = us.Id
                               left join Users agents on SupportTickets.AgentId = agents.Id
                               where SupportTickets.Id = @id";
        const string messagesQuery = @"select * from Messages 
                                       inner join Users users on Messages.UserId = users.Id 
                                       where SupportTicketId = @Id";

        const string solutionsQuery = @"select * from Solutions
                                        inner join Messages messages on Solutions.MessageId = messages.Id
                                        where messages.SupportTicketId = @Id";

        SupportTicket? supportTicket = null;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            supportTicket = (await transaction.QueryAsync<SupportTicket, User, User, SupportTicket>(
                    sql: query,
                    map: MapSupportTicketQuery,
                    param: new { id }))
                .FirstOrDefault();

            if (supportTicket == null)
                return;

            supportTicket.Messages =
                await transaction.QueryAsync<Message, User, Message>(messagesQuery, MapMessagesQuery,
                    new { supportTicket.Id });

            supportTicket.Solutions =
                await transaction.QueryAsync<Solution>(solutionsQuery, new { supportTicket.Id });
        });
        return supportTicket;
    }

    public async Task<bool> IsExistsAsync(Guid id)
    {
        const string query = "select exists(select * from SupportTickets where Id = @id)";
        await using DbConnection connection = _dbContext.CreateConnection();
        var exists = false;
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            exists = await transaction.QuerySingleAsync<bool>(query, new { id });
        });
        return exists;
    }

    public async Task InsertAsync(SupportTicket item)
    {
        const string query = "insert into SupportTickets values (@Id, @Description, @UserId, @AgentId, @Status)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            var param = new
            {
                item.Id, 
                item.Description, 
                UserId = item.User.Id,
                AgentId = item.Agent?.Id,
                Status = item.Status.ToString()
            };
            await transaction.ExecuteAsync(query, param);
        });
    }

    public async Task UpdateAsync(SupportTicket item)
    {
        const string query = @"update SupportTickets set 
                               Description = @Description, 
                               AgentId = @AgentId,
                               Status = @Status
                               where Id = @Id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(
            sql: query,
            param: new { item.Id, item.Description, AgentId = item.Agent?.Id, Status = item.Status.ToString() }));
    }

    public async Task DeleteAsync(Guid id)
    {
        const string query = "delete from SupportTickets where Id = @id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, param: new { id });
        });
    }

    public async Task<IEnumerable<SupportTicket>> GetBasedOnAccountAsync(Guid accountId)
    {
        const string query = @"select * from SupportTickets
                               inner join Users us on SupportTickets.UserId = us.Id
                               left join Users agents on SupportTickets.AgentId = agents.Id
                               where SupportTickets.UserId = @accountId or SupportTickets.AgentId = @accountId";
        const string messagesQuery = @"select * from Messages 
                                       inner join Users users on Messages.UserId = users.Id
                                       where SupportTicketId = @Id";
        const string solutionsQuery = @"select * from Solutions
                                        inner join Messages messages on Solutions.MessageId = messages.Id
                                        where messages.SupportTicketId = @Id";

        IEnumerable<SupportTicket> supportTickets = null!;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            supportTickets = (await transaction.QueryAsync<SupportTicket, User, User, SupportTicket>(
                    sql: query,
                    map: MapSupportTicketQuery,
                    param: new { accountId }))
                .ToList();

            if (!supportTickets.Any())
                return;

            foreach (SupportTicket supportTicket in supportTickets)
            {
                supportTicket.Messages =
                    await transaction.QueryAsync<Message, User, Message>(messagesQuery, MapMessagesQuery,
                        new { supportTicket.Id });

                supportTicket.Solutions =
                    await transaction.QueryAsync<Solution>(solutionsQuery, new { supportTicket.Id });
            }
        });
        return supportTickets;
    }

    private static SupportTicket MapSupportTicketQuery(SupportTicket supportTicket, User user, User agent)
    {
        supportTicket.User = user;
        supportTicket.Agent = agent;
        return supportTicket;
    }


    public async Task<Message?> GetMessageByIdAsync(Guid messageId)
    {
        const string query =
            "select * from Messages inner join Users users on Messages.UserId = users.Id  where Messages.Id = @messageId";

        Message? message = null;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            message = (await transaction.QueryAsync<Message, User, Message>(
                sql: query,
                map: MapMessagesQuery,
                param: new { messageId })).FirstOrDefault();
        });
        return message;
    }

    private static Message MapMessagesQuery(Message message, User user)
    {
        message.User = user;
        return message;
    }


    public async Task AddMessageAsync(Message message)
    {
        const string query = "insert into Messages values (@Id, @SupportTicketId, @UserId, @Content)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            var param = new { message.Id, message.SupportTicketId, UserId = message.User.Id, message.Content };
            await transaction.ExecuteAsync(query, param);
        });
    }

    public async Task UpdateMessageAsync(Message message)
    {
        const string query = "update Messages set Content = @Content where Id = @Id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, message));
    }

    public async Task DeleteMessageAsync(Guid messageId)
    {
        const string query = "delete from Messages where Id = @messageId";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, param: new { messageId });
        });
    }

    public async Task AddSolutionAsync(Solution solution)
    {
        const string query = "insert into Solutions values (@MessageId, @Status)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            var param = new { solution.MessageId, Status = solution.Status.ToString() };
            await transaction.ExecuteAsync(query, param);
        });
    }

    public async Task UpdateSolutionAsync(Solution solution)
    {
        const string query = "update Solutions set Status = @Status where MessageId = @MessageId";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, new { solution.MessageId, Status = solution.Status.ToString() });
        });
    }
}