using System.ComponentModel.DataAnnotations;
using SupportTickets.Api.Models.Images;

namespace SupportTickets.Api.Models.Messages;

public class MessageCreate
{
    [Required] public Guid SupportTicketId { get; set; }
    [Required] [MaxLength(1000)] public string Content { get; set; }
    public IEnumerable<ImageCreate>? Images { get; set; }
}