using System.ComponentModel.DataAnnotations;

namespace HelpDeskSystem.Models.SupportTicket;

public class SupportTicketCreate
{
    [Required] [MaxLength(500)] public string Description { get; set; }
}