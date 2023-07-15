using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Models;

public class RabbitMqConfiguration
{
    [Required] public string Host { get; set; }
    [Required] public int Port { get; set; }
    [Required] public string UserName { get; set; }
    [Required] public string Password { get; set; }
    [Required] public bool AutomaticRecovery { get; set; }
    [Required] public string ExchangeName { get; set; }
    [Required] public int ConnectionRetryCount { get; set; }
    [Required] public TimeSpan ConnectionRetrySleepDuration { get; set; }
}