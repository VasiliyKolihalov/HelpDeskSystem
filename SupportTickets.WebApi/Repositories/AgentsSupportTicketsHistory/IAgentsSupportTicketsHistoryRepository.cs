using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Repositories.AgentsSupportTicketsHistory;

public interface IAgentsSupportTicketsHistoryRepository
{
    public Task<IEnumerable<User>> GetBySupportTicketIdAsync(Guid supportTicketId);
    public Task InsertAsync(Guid supportTicketId, Guid userId);
}