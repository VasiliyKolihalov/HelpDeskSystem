namespace SupportTickets.Api.Services.JobsManagers.Escalations;

public interface ISupportTicketsEscalationManager
{
    public void AssignEscalationFor(Guid supportTicketId, TimeSpan afterTime);
    public void CancelEscalationFor(Guid supportTicket);
}