using Dapper.Transaction;
using Infrastructure.Extensions;
using Npgsql;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Repositories.SupportTickets;

public class SupportTicketsRepository : ISupportTicketsRepository
{
    private readonly string _connectionString;

    public SupportTicketsRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<SupportTicket>> GetAllAsync()
    {
        const string query = "select * from SupportTickets";
        IEnumerable<SupportTicket> tickets = null!;
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            tickets = await transaction.QueryAsync<SupportTicket>(query);
        });
        return tickets;
    }

    public async Task<SupportTicket?> GetByIdAsync(Guid id)
    {
        const string query = @"select * from SupportTickets
                                          inner join Users on SupportTickets.UserId = Users.Id
                                          where SupportTickets.Id = @id";
        SupportTicket? ticket = null;
        await using var connection = new NpgsqlConnection(_connectionString);
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
        const string query = "insert into SupportTickets values (@Id, @Description, @UserId)";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            var param = new { item.Id, item.Description, UserId = item.User.Id };
            await transaction.ExecuteAsync(query, param);
        });
    }

    public async Task UpdateAsync(SupportTicket item)
    {
        const string query = "update SupportTickets set Description = @Description where Id = @Id";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, item));
    }

    public async Task DeleteAsync(Guid id)
    {
        const string query = "delete from SupportTickets where Id = @id";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, param: new { id });
        });
    }
}