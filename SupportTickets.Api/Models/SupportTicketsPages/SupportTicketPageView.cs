using SupportTickets.Api.Models.SupportTickets;

namespace SupportTickets.Api.Models.SupportTicketsPages;

public class SupportTicketPageView
{
    public IEnumerable<SupportTicketPreview> SupportTickets { get; set; }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public SupportTicketStatus? Status { get; set; }
    public SupportTicketPriority? Priority { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}