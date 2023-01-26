using System.ComponentModel.DataAnnotations;

namespace HelpDeskSystem.Models;

public class SupportTicketUpdate
{
    [Required] public Guid Id { get; set; }
    [Required] [MaxLength(500)] public string Description { get; set; }
}