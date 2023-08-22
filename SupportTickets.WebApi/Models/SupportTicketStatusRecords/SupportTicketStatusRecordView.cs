using SupportTickets.WebApi.Models.SupportTickets;

namespace SupportTickets.WebApi.Models.SupportTicketStatusRecords;

public class SupportTicketStatusRecordView
{
    public Guid SupportTicketId { get; set; }
    public SupportTicketStatus Status { get; set; }
    public DateTime DateTime { get; set; }
}