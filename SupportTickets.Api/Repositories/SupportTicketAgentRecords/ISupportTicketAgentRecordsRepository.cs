using SupportTickets.Api.Models.SupportTicketAgentRecords;

namespace SupportTickets.Api.Repositories.SupportTicketAgentRecords;

public interface ISupportTicketAgentRecordsRepository
{
    public Task<IEnumerable<SupportTicketAgentRecord>> GetBySupportTicketIdAsync(Guid supportTicketId);
    public Task InsertAsync(SupportTicketAgentRecord record);
}