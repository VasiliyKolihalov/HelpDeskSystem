using System.Data.SqlClient;
using Dapper.Transaction;
using Infrastructure.Extensions;
using Npgsql;
using SupportTicketWebApi.Models;

namespace SupportTicketWebApi.Repositories;

public class SupportTicketRepository : ISupportTicketRepository
{
    private readonly string _connectionString;

    public SupportTicketRepository(string connectionString)
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

    public async Task<SupportTicket> GetByIdAsync(Guid id)
    {
        const string query = "select * from SupportTickets where Id = @id";
        SupportTicket? ticket = null;
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            ticket = await transaction.QuerySingleOrDefaultAsync<SupportTicket>(query, param: new { id });
        });
        return ticket;
    }

    public async Task InsertAsync(SupportTicket item)
    {
        const string query = "insert into SupportTickets values (@Id, @Description)";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, item));
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