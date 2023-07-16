using System.Data.Common;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Infrastructure.Services.Persistence;
using Npgsql;
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
        const string query = "select * from supporttickets";
        IEnumerable<SupportTicket> tickets = null!;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            tickets = await transaction.QueryAsync<SupportTicket>(query);
        });
        return tickets;
    }

    public async Task<SupportTicket?> GetByIdAsync(Guid id)
    {
        const string query = @"select * from supporttickets
                                          inner join users on supporttickets.userid = users.id
                                          where supporttickets.id = @id";
        SupportTicket? ticket = null;
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            ticket = (await transaction.QueryAsync<SupportTicket, User, SupportTicket>(
                    sql: query,
                    map: (supportTicket, user) =>
                    {
                        supportTicket.User = user;
                        return supportTicket;
                    }, param: new { id }))
                .FirstOrDefault();
        });
        return ticket;
    }

    public async Task InsertAsync(SupportTicket item)
    {
        const string query = "insert into supporttickets values (@Id, @Description, @UserId)";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            var param = new { item.Id, item.Description, UserId = item.User.Id };
            await transaction.ExecuteAsync(query, param);
        });
    }

    public async Task UpdateAsync(SupportTicket item)
    {
        const string query = "update supporttickets set description = @Description where id = @Id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, item));
    }

    public async Task DeleteAsync(Guid id)
    {
        const string query = "delete from supporttickets where id = @id";
        await using DbConnection connection = _dbContext.CreateConnection();
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, param: new { id });
        });
    }
}