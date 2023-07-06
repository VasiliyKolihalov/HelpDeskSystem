using SupportTicketWebApi.Models;

namespace SupportTicketWebApi.Repositories;

public interface ISupportTicketRepository
{
    public Task<IEnumerable<SupportTicket>> GetAllAsync();
    public Task<SupportTicket?> GetByIdAsync(Guid id);
    public Task InsertAsync(SupportTicket item);
    public Task UpdateAsync(SupportTicket item);
    public Task DeleteAsync(Guid id);
}