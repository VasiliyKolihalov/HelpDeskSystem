using SupportTickets.WebApi.Models.Messages;
using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Models.SupportTickets;

public class SupportTicket
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public User User { get; set; }
    public User? Agent { get; set; }
    
    public IEnumerable<Message>? Messages { get; set; }
}