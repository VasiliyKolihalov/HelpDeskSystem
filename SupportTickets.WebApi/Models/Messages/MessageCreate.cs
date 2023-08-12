using System.ComponentModel.DataAnnotations;

namespace SupportTickets.WebApi.Models.Messages;

public class MessageCreate
{
    [Required] public Guid SupportTicketId { get; set; }
    [Required] [MaxLength(1000)] public string Content { get; set; }
}