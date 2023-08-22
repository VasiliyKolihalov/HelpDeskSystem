using SupportTickets.WebApi.Models.SupportTicketAgentRecords;

namespace SupportTickets.WebApi.Repositories.SupportTicketAgentRecords;

public interface ISupportTicketAgentRecordsRepository
{
    public Task<IEnumerable<SupportTicketAgentRecord>> GetBySupportTicketIdAsync(Guid supportTicketId);
    public Task InsertAsync(SupportTicketAgentRecord record);
}