using SupportTickets.WebApi.Models.SupportTicketStatusRecords;

namespace SupportTickets.WebApi.Repositories.SupportTicketStatusRecords;

public interface ISupportTicketStatusRecordsRepository
{
    public Task<IEnumerable<SupportTicketStatusRecord>> GetBySupportTicketIdAsync(Guid supportTicketId);
    public Task InsertAsync(SupportTicketStatusRecord record);
}