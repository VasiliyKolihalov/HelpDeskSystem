using SupportTickets.WebApi.Models.Messages;
using SupportTickets.WebApi.Models.Solutions;
using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Models.SupportTickets;

public class SupportTicket
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public User User { get; set; }
    public User? Agent { get; set; }
    public SupportTicketStatus Status { get; set; }
    public SupportTicketPriority Priority { get; set; }
    
    public IEnumerable<Message> Messages { get; set; }
    public IEnumerable<Solution> Solutions { get; set; }
}

public enum SupportTicketStatus
{
    Open,
    Solved,
    Close
}

public enum SupportTicketPriority
{
    Low,
    Medium,
    High
    
}