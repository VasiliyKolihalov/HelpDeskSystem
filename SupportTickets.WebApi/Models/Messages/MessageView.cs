using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Models.Messages;

public class MessageView
{
    public Guid Id { get; set; }
    public UserView User { get; set; }
    public string Content { get; set; }
}