namespace SupportTickets.Api.Models.Solutions;

public class SolutionView
{
    public Guid MessageId { get; set; }
    public SolutionStatus Status { get; set; }
}