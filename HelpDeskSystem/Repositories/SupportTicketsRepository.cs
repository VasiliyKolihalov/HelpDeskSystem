using Dapper.Transaction;
using HelpDeskSystem.Exceptions;
using HelpDeskSystem.Models;

namespace HelpDeskSystem.Repositories;

public class SupportTicketsRepository : Repository<SupportTicket, Guid>
{
    public SupportTicketsRepository(string connectionString) : base(connectionString)
    {
    }

    public override async Task<IEnumerable<SupportTicket>> GetAllAsync()
    {
        const string query = "select * from SupportTickets";
        IEnumerable<SupportTicket> tickets = null!;
        await ExecuteTransactionAsync(async transaction =>
        {
            tickets = await transaction.QueryAsync<SupportTicket>(query);
        });
        return tickets;
    }

    public override async Task<SupportTicket> GetByIdAsync(Guid id)
    {
        const string query = "select * from SupportTickets where Id = @id";
        SupportTicket? ticket = null;
        await ExecuteTransactionAsync(async transaction =>
        {
            ticket = await transaction.QueryFirstOrDefaultAsync<SupportTicket>(query, new { id });
        });
        return ticket ?? throw new NotFoundException("Support ticket not found");
    }

    public override async Task InsertAsync(SupportTicket item)
    {
        const string query = "insert into SupportTickets values (@Id, @Description, @Datetime)";
        await ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, item));
    }

    public override async Task UpdateAsync(SupportTicket item)
    {
        const string query = @"update SupportTickets set Description = @Description where Id = @Id";

        await ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, item));
    }

    public override async Task DeleteAsync(Guid id)
    {
        const string query = "delete from SupportTickets where Id = @id";
        await ExecuteTransactionAsync(async transaction => await transaction.ExecuteAsync(query, new { id }));
    }
}