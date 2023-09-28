using System.ComponentModel.DataAnnotations;

namespace SupportTickets.Api.Models.SupportTickets;

public class SupportTicketCreate
{
    [Required] [MaxLength(500)] public string Description { get; set; }
}