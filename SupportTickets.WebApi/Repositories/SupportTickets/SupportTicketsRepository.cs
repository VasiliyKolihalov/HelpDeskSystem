using System.Data.Common;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;
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
        const string query = @"select * from SupportTickets
                              inner join Users users on SupportTickets.UserId = users.Id";
        IEnumerable<SupportTicket> tickets = null!;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            tickets = await transaction.QueryAsync<SupportTicket, User, SupportTicket>(query, MapQuery);
        });
        return tickets;
    }

    public async Task<SupportTicket?> GetByIdAsync(Guid id)
    {
        const string query = @"select * from SupportTickets
                                          inner join Users users on SupportTickets.UserId = users.Id
                                          where SupportTickets.id = @id";
        SupportTicket? ticket = null;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            ticket = (await transaction.QueryAsync<SupportTicket, User, SupportTicket>(
                    sql: query,
                    map: MapQuery,
                    param: new { id }))
                .FirstOrDefault();
        });
        return ticket;
    }

    private static SupportTicket MapQuery(SupportTicket supportTicket, User user)
    {
        supportTicket.User = user;
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
        const string query = "insert into SupportTickets values (@Id, @Description, @UserId)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            var param = new { item.Id, item.Description, UserId = item.User.Id };
            await transaction.ExecuteAsync(query, param);
        });
    }

    public async Task UpdateAsync(SupportTicket item)
    {
        const string query = "update SupportTickets set Description = @Description where Id = @Id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, item));
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
}