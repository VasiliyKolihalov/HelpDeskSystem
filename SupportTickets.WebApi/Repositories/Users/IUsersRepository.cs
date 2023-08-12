using Infrastructure.Repositories;
using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Repositories.Users;

public interface IUsersRepository : IRepository<User, Guid>
{
}