using SupportTickets.Api.Models.Messages;
using SupportTickets.Api.Models.Solutions;
using SupportTickets.Api.Models.Users;

namespace SupportTickets.Api.Models.SupportTickets;

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