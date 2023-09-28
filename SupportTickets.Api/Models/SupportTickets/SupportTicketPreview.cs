namespace SupportTickets.Api.Models.SupportTickets;

public class SupportTicketPreview
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public SupportTicketStatus Status { get; set; }
    public SupportTicketPriority Priority { get; set; }
}