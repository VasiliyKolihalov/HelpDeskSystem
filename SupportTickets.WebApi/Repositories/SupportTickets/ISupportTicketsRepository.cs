using Infrastructure.Repositories;
using SupportTickets.WebApi.Models.Messages;
using SupportTickets.WebApi.Models.Solutions;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Repositories.SupportTickets;

public interface ISupportTicketsRepository : IRepository<SupportTicket, Guid>
{
    public Task<IEnumerable<SupportTicket>> GetBasedOnAccountAsync(Guid accountId);
    public Task<IEnumerable<SupportTicket>> GetAllWithoutAgent();

    public Task<Message?> GetMessageByIdAsync(Guid messageId);
    public Task AddMessageAsync(Message message);
    public Task UpdateMessageAsync(Message message);
    public Task DeleteMessageAsync(Guid messageId);

    public Task<IEnumerable<User>> GetAgentsHistoryAsync(Guid supportTicketId);
    public Task AddToAgentsHistoryAsync(Guid supportTicketId, Guid userId);

    public Task AddSolutionAsync(Solution solution);
    public Task UpdateSolutionAsync(Solution solution);
}