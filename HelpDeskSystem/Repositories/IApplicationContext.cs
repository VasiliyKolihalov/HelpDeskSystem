using HelpDeskSystem.Models.SupportTicket;

namespace HelpDeskSystem.Repositories;

public interface IApplicationContext
{
    public IRepository<SupportTicket, Guid> SupportTickets { get; }
    public IUsersRepository Users { get; }
    public IRolesRepository Roles { get; }
}