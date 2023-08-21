namespace SupportTickets.WebApi.Services.JobsManagers.Closing;

public interface ISupportTicketsClosingManager
{
    public void EnsureAssignCloseFor(Guid supportTicketId, TimeSpan afterTime);
    public void EnsureCancelCloseFor(Guid supportTicketId);
}