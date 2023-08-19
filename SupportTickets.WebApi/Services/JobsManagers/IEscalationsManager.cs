namespace SupportTickets.WebApi.Services.JobsManagers;

public interface IEscalationsManager
{
    public void AssignEscalationFor(Guid supportTicketId, TimeSpan afterTime);
    public void CancelEscalationFor(Guid supportTicket);
}