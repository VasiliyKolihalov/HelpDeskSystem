using Infrastructure.Repositories;
using SupportTickets.Api.Models.Users;

namespace SupportTickets.Api.Repositories.Users;

public interface IUsersRepository : IRepository<User, Guid>
{
}