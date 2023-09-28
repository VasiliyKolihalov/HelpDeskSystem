namespace SupportTickets.Api.Models.Solutions;

public class Solution
{
    public Guid MessageId { get; set; }
    public SolutionStatus Status { get; set; }
}

public enum SolutionStatus
{
    Suggested,
    Accepted,
    Rejected
}