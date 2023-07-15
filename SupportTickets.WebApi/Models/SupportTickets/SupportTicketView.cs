using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Models.SupportTickets;

public class SupportTicketView
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public UserView User { get; set; }
}