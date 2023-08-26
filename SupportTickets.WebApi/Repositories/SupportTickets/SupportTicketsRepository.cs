using System.Data.Common;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;
using SupportTickets.WebApi.Models.Messages;
using SupportTickets.WebApi.Models.Solutions;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Models.Users;
using SupportTickets.WebApi.Services;

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
        const string supportTicketsQuery = "select * from SupportTickets " +
                                           "inner join Users us on SupportTickets.UserId = us.Id " +
                                           "left join Users agents on SupportTickets.AgentId = agents.Id";
        const string messagesQuery = "select * from Messages " +
                                     "inner join Users users on Messages.UserId = users.Id " +
                                     "where SupportTicketId = @Id";

        const string solutionsQuery = "select * from Solutions " +
                                      "inner join Messages messages on Solutions.MessageId = messages.Id " +
                                      "where messages.SupportTicketId = @Id";

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
        const string query = "select * from SupportTickets " +
                             "inner join Users us on SupportTickets.UserId = us.Id " +
                             "left join Users agents on SupportTickets.AgentId = agents.Id " +
                             "where SupportTickets.Id = @id";
        const string messagesQuery = "select * from Messages " +
                                     "inner join Users users on Messages.UserId = users.Id " +
                                     "where SupportTicketId = @Id";

        const string solutionsQuery = "select * from Solutions " +
                                      "inner join Messages messages on Solutions.MessageId = messages.Id " +
                                      "where messages.SupportTicketId = @Id";

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
        const string query = "insert into SupportTickets values (" +
                             "@Id, " +
                             "@Description, " +
                             "@UserId, " +
                             "@AgentId, " +
                             "@Status, " +
                             "@Priority)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            var param = new
            {
                item.Id,
                item.Description,
                UserId = item.User.Id,
                AgentId = item.Agent?.Id,
                Status = item.Status.ToString(),
                Priority = item.Priority.ToString()
            };
            await transaction.ExecuteAsync(query, param);
        });
    }

    public async Task UpdateAsync(SupportTicket item)
    {
        const string query = "update SupportTickets " +
                             "set Description = @Description, " +
                             "AgentId = @AgentId, " +
                             "Status = @Status, " +
                             "Priority = @Priority " +
                             "where Id = @Id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(
            sql: query,
            param: new
            {
                item.Id,
                item.Description,
                AgentId = item.Agent?.Id,
                Status = item.Status.ToString(),
                Priority = item.Priority.ToString()
            }));
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
        const string query = "select * from SupportTickets " +
                             "inner join Users us on SupportTickets.UserId = us.Id " +
                             "left join Users agents on SupportTickets.AgentId = agents.Id " +
                             "where SupportTickets.UserId = @accountId or SupportTickets.AgentId = @accountId";
        const string messagesQuery = "select * from Messages " +
                                     "inner join Users users on Messages.UserId = users.Id " +
                                     "where SupportTicketId = @Id";
        const string solutionsQuery = "select * from Solutions " +
                                      "inner join Messages messages on Solutions.MessageId = messages.Id " +
                                      "where messages.SupportTicketId = @Id";

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

    public async Task<IEnumerable<SupportTicket>> GetAllOpenWithoutAgent()
    {
        const string supportTicketsQuery = "select * from SupportTickets supportTickets " +
                                           "inner join Users us on SupportTickets.UserId = us.Id " +
                                           "where supportTickets.AgentId is null and supportTickets.status = 'Open'";

        const string messagesQuery = "select * from Messages " +
                                     "inner join Users users on Messages.UserId = users.Id " +
                                     "where SupportTicketId = @Id";

        const string solutionsQuery = "select * from Solutions " +
                                      "inner join Messages messages on Solutions.MessageId = messages.Id " +
                                      "where messages.SupportTicketId = @Id";

        IEnumerable<SupportTicket> tickets = null!;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            tickets = (await transaction.QueryAsync<SupportTicket, User, SupportTicket>(
                sql: supportTicketsQuery,
                map: (supportTicket, user) =>
                {
                    supportTicket.User = user;
                    return supportTicket;
                })).ToList();

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

    public async Task<IEnumerable<SupportTicket>> GetByPagination(
        Action<ISupportTicketsPaginationQueueBuilder> builderAction)
    {
        var queueBuilder = new SupportTicketsPaginationQueueBuilder("select * from SupportTickets supporttickets " +
                                                                    "inner join Users us on SupportTickets.UserId = us.Id " +
                                                                    "left join Users agents on SupportTickets.AgentId = agents.Id ");
        builderAction(queueBuilder);

        string supportTicketsQuery = queueBuilder.Build();

        const string messagesQuery = "select * from Messages " +
                                     "inner join Users users on Messages.UserId = users.Id " +
                                     "where SupportTicketId = @Id";

        const string solutionsQuery = "select * from Solutions " +
                                      "inner join Messages messages on Solutions.MessageId = messages.Id " +
                                      "where messages.SupportTicketId = @Id";

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

    public async Task<int> GetCountByPagination(
        Action<ISupportTicketsPaginationQueueBuilder> builderAction)
    {
        var queueBuilder = new SupportTicketsPaginationQueueBuilder("select count(*) from SupportTickets ");
        builderAction(queueBuilder);

        string supportTicketsQuery = queueBuilder.Build();

        var count = 0;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            count = await transaction.QuerySingleAsync<int>(sql: supportTicketsQuery);
        });
        return count;
    }

    private static SupportTicket MapSupportTicketQuery(SupportTicket supportTicket, User user, User agent)
    {
        supportTicket.User = user;
        supportTicket.Agent = agent;
        return supportTicket;
    }

    private static Message MapMessagesQuery(Message message, User user)
    {
        message.User = user;
        return message;
    }
}