using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Repositories.Users;

public interface IUsersRepository
{
    public Task<User?> GetByIdAsync(Guid id);
    public Task InsertAsync(User user);
    public Task UpdateAsync(User user);
    public Task DeleteAsync(Guid id);
}