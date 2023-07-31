using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Models;

public class PollyOptions
{
    [Required] public int? RetryCount { get; set; }
    [Required] public TimeSpan? RetrySleepDuration { get; set; }
}