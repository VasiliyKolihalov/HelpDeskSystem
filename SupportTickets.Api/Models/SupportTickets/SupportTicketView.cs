using SupportTickets.Api.Models.Messages;
using SupportTickets.Api.Models.Solutions;
using SupportTickets.Api.Models.Users;

namespace SupportTickets.Api.Models.SupportTickets;

public class SupportTicketView
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public UserView User { get; set; }
    public UserView? Agent { get; set; }
    public SupportTicketStatus Status { get; set; }
    public SupportTicketPriority Priority { get; set; }

    public IEnumerable<MessageView>? Messages { get; set; }
    public IEnumerable<SolutionView>? Solutions { get; set; }
}