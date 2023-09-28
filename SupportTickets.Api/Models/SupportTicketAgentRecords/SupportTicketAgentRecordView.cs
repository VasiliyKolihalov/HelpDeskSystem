namespace SupportTickets.Api.Models.SupportTicketAgentRecords;

public class SupportTicketAgentRecordView
{
    public Guid SupportTicketId { get; set; }
    public Guid AgentId { get; set; }
    public DateTime DateTime { get; set; }
}