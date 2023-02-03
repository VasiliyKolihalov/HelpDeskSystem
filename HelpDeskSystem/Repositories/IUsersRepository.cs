using HelpDeskSystem.Models.User;

namespace HelpDeskSystem.Repositories;

public interface IUsersRepository : IRepository<User, Guid>
{
    public Task<User?> GetByEmailAsync(string email);
}