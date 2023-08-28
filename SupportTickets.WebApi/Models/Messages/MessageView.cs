using SupportTickets.WebApi.Models.Images;
using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Models.Messages;

public class MessageView
{
    public Guid Id { get; set; }
    public UserView User { get; set; }
    public string Content { get; set; }
    public IEnumerable<ImageView>? Images { get; set; }
}