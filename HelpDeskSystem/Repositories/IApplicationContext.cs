namespace HelpDeskSystem.Repositories;

public interface IApplicationContext
{
    public SupportTicketsRepository SupportTickets { get; }
}