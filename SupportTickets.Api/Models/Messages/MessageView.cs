using SupportTickets.Api.Models.Images;
using SupportTickets.Api.Models.Users;

namespace SupportTickets.Api.Models.Messages;

public class MessageView
{
    public Guid Id { get; set; }
    public UserView User { get; set; }
    public string Content { get; set; }
    public IEnumerable<ImageView>? Images { get; set; }
    public DateTime DateTime { get; set; }
}