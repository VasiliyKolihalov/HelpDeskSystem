using System.ComponentModel.DataAnnotations;

namespace SupportTickets.WebApi.Models.SupportTickets;

public class SupportTicketUpdate
{
    [Required] public Guid Id { get; set; }
    [Required] [MaxLength(500)] public string Description { get; set; }
}