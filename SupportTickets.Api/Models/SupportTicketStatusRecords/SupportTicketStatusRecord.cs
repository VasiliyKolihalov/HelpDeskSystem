using SupportTickets.Api.Models.SupportTickets;

namespace SupportTickets.Api.Models.SupportTicketStatusRecords;

public class SupportTicketStatusRecord
{
    public Guid SupportTicketId { get; set; }
    public SupportTicketStatus Status { get; set; }
    public DateTime DateTime { get; set; }
}