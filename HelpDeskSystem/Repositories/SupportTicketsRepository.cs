using System.Data.SqlClient;
using Dapper.Transaction;
using HelpDeskSystem.Exceptions;
using HelpDeskSystem.Extensions;
using HelpDeskSystem.Models.SupportTicket;

namespace HelpDeskSystem.Repositories;

public class SupportTicketsRepository : IRepository<SupportTicket, Guid>
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
        await using var connection = new SqlConnection(_connectionString);
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
        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            ticket = await transaction.QuerySingleOrDefaultAsync<SupportTicket>(query, new { id });
        });
        return ticket ?? throw new NotFoundException("Support ticket not found");
    }

    public async Task InsertAsync(SupportTicket item)
    {
        const string query = "insert into SupportTickets values (@Id, @Description, @Datetime)";
        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, item));
    }

    public async Task UpdateAsync(SupportTicket item)
    {
        const string query = @"update SupportTickets set Description = @Description where Id = @Id";
        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, item));
    }

    public async Task DeleteAsync(Guid id)
    {
        const string query = "delete from SupportTickets where Id = @id";
        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteTransactionAsync(async transaction =>
        {
            await transaction.ExecuteAsync(query, new { id });
        });
    }
}