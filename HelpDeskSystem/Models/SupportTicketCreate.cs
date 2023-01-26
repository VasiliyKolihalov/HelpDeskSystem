using System.ComponentModel.DataAnnotations;

namespace HelpDeskSystem.Models;

public class SupportTicketCreate
{
    [Required] [MaxLength(500)] public string Description { get; set; }
}