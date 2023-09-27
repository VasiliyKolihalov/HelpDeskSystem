using SupportTickets.Api.Models.Users;

namespace SupportTickets.Api.Models.Messages;

public class Message
{
    public Guid Id { get; set; }
    public Guid SupportTicketId { get; set; }
    public User User { get; set; }
    public string Content { get; set; }
    public DateTime DateTime { get; set; }
}