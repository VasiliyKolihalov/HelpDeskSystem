namespace HelpDeskSystem.Repositories;

public class ApplicationContext : IApplicationContext
{
    public SupportTicketsRepository SupportTickets { get; }

    public ApplicationContext(string connectionString)
    {
        SupportTickets = new SupportTicketsRepository(connectionString);
    }
}