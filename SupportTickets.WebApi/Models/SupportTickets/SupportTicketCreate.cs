using System.ComponentModel.DataAnnotations;

namespace SupportTickets.WebApi.Models.SupportTickets;

public class SupportTicketCreate
{
    [Required] [MaxLength(500)] public string Description { get; set; }
}