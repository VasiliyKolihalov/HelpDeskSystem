using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Models.Messages;

public class Message
{
    public Guid Id { get; set; }
    public Guid SupportTicketId { get; set; }
    public User User { get; set; }
    public string Content { get; set; }
}