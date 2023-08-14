namespace SupportTickets.WebApi.Models.SupportTickets;

public class SupportTicketPreview
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public bool IsClose { get; set; }
}