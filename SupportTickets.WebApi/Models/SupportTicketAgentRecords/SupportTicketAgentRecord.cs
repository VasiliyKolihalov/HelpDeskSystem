namespace SupportTickets.WebApi.Models.SupportTicketAgentRecords;

public class SupportTicketAgentRecord
{
    public Guid SupportTicketId { get; set; }
    public Guid AgentId { get; set; }
    public DateTime DateTime { get; set; }
}