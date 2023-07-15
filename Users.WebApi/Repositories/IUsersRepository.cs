using Infrastructure.Repositories;
using Users.WebApi.Models.Users;

namespace Users.WebApi.Repositories;

public interface IUsersRepository : IRepository<User, Guid>
{
    
}