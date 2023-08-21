using Infrastructure.Repositories;
using SupportTickets.WebApi.Models.SupportTickets;

namespace SupportTickets.WebApi.Repositories.SupportTickets;

public interface ISupportTicketsRepository : IRepository<SupportTicket, Guid>
{
    public Task<IEnumerable<SupportTicket>> GetBasedOnAccountAsync(Guid accountId);
    public Task<IEnumerable<SupportTicket>> GetAllOpenWithoutAgent();
}