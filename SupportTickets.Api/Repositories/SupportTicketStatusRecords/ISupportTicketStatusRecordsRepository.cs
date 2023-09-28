using SupportTickets.Api.Models.SupportTicketStatusRecords;

namespace SupportTickets.Api.Repositories.SupportTicketStatusRecords;

public interface ISupportTicketStatusRecordsRepository
{
    public Task<IEnumerable<SupportTicketStatusRecord>> GetBySupportTicketIdAsync(Guid supportTicketId);
    public Task InsertAsync(SupportTicketStatusRecord record);
}