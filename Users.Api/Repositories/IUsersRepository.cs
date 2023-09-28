using Infrastructure.Repositories;
using Users.Api.Models.Users;

namespace Users.Api.Repositories;

public interface IUsersRepository : IRepository<User, Guid>
{
    
}