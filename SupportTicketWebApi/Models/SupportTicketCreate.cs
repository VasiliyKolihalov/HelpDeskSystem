using System.ComponentModel.DataAnnotations;

namespace SupportTicketWebApi.Models;

public class SupportTicketCreate
{
    [Required] [MaxLength(500)] public string Description { get; set; }
}