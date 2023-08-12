using System.ComponentModel.DataAnnotations;

namespace SupportTickets.WebApi.Models.Messages;

public class MessageUpdate
{
    [Required] public Guid Id { get; set; }
    [Required] [MaxLength(1000)] public string Content { get; set; }
}