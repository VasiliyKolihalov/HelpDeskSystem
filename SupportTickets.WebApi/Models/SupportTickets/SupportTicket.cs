using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Models.SupportTickets;

public class SupportTicket
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public User User { get; set; }
}