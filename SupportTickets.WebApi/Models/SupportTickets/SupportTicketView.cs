using SupportTickets.WebApi.Models.Messages;
using SupportTickets.WebApi.Models.Solutions;
using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Models.SupportTickets;

public class SupportTicketView
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public UserView User { get; set; }
    public UserView? Agent { get; set; }
    public SupportTicketStatus Status { get; set; }

    public List<MessageView>? Messages { get; set; }
    public IEnumerable<SolutionView>? Solutions { get; set; }
}