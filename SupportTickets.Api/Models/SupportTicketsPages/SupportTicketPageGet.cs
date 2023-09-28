using System.ComponentModel.DataAnnotations;
using Infrastructure.Attributes;
using SupportTickets.Api.Models.SupportTickets;

namespace SupportTickets.Api.Models.SupportTicketsPages;

public class SupportTicketPageGet
{
    [Required] [Min(1)] public int PageNumber { get; set; }
    [Required] [Min(1)] public int PageSize { get; set; }
    public SupportTicketStatus? Status { get; set; }
    public SupportTicketPriority? Priority { get; set; }
}