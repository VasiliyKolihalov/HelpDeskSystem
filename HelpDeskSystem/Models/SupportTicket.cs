namespace HelpDeskSystem.Models;

public class SupportTicket
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public DateTime DateTime { get; set; }
}