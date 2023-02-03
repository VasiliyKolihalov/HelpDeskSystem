using HelpDeskSystem.Models.SupportTicket;

namespace HelpDeskSystem.Repositories;

public class ApplicationContext : IApplicationContext
{
    public IRepository<SupportTicket, Guid> SupportTickets { get; }
    public IUsersRepository Users { get; }
    public IRolesRepository Roles { get; }

    public ApplicationContext(string connectionString)
    {
        SupportTickets = new SupportTicketsRepository(connectionString);
        Users = new UsersRepository(connectionString);
        Roles = new RolesRepository(connectionString);
    }
}