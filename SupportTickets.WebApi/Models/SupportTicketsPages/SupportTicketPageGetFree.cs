using System.ComponentModel.DataAnnotations;
using Infrastructure.Attributes;
using SupportTickets.WebApi.Models.SupportTickets;

namespace SupportTickets.WebApi.Models.SupportTicketsPages;

public class SupportTicketPageGetFree
{
    [Required] [Min(1)] public int PageNumber { get; set; }
    [Required] [Min(1)] public int PageSize { get; set; }
    public SupportTicketPriority? Priority { get; set; }
}