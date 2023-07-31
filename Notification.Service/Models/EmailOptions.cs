using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models.Options;

public class EmailOptions
{
    [Required] public string Host { get; set; }
    [Required] public int? Port { get; set; }
    [Required] public bool? IsNeedSsl { get; set; }
    [Required] public string SenderName { get; set; }
    [Required] public string SenderEmail { get; set; }
    [Required] public string SenderPassword { get; set; }
}