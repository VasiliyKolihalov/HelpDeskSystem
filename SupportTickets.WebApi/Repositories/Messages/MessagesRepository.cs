using System.Data.Common;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;
using SupportTickets.WebApi.Models.Messages;
using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Repositories.Messages;

public class MessagesRepository : IMessagesRepository
{
    private readonly IDbContext _dbContext;

    public MessagesRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Message>> GetAllAsync()
    {
        const string query = "select * from Messages " +
                             "inner join Users users on Messages.UserId = users.Id";
        IEnumerable<Message> messages = null!;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            messages = await transaction.QueryAsync<Message, User, Message>(sql: query, map: MapMessagesQuery);
        });
        return messages;
    }

    public async Task<Message?> GetByIdAsync(Guid id)
    {
        const string query = "select * from Messages " +
                             "inner join Users users on Messages.UserId = users.Id " +
                             "where Messages.Id = @id";

        Message? message = null;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            message = (await transaction.QueryAsync<Message, User, Message>(
                sql: query,
                map: MapMessagesQuery,
                param: new { id })).FirstOrDefault();
        });
        return message;
    }

    public async Task<bool> IsExistsAsync(Guid id)
    {
        const string query = "select exists(select * from Messages where Id = @id)";
        var exists = false;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            exists = await transaction.QuerySingleAsync<bool>(query, new { id });
        });
        return exists;
    }

    public async Task InsertAsync(Message item)
    {
        const string query = "insert into Messages values (@Id, @SupportTicketId, @UserId, @Content)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            var param = new { item.Id, item.SupportTicketId, UserId = item.User.Id, item.Content };
            await transaction.ExecuteAsync(query, param);
        });
    }

    public async Task UpdateAsync(Message item)
    {
        const string query = "update Messages set Content = @Content where Id = @Id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, item));
    }

    public async Task DeleteAsync(Guid id)
    {
        const string query = "delete from Messages where Id = @id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, param: new { id });
        });
    }

    private static Message MapMessagesQuery(Message message, User user)
    {
        message.User = user;
        return message;
    }
}