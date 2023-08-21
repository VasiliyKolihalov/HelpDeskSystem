using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Services.Users;

public interface IUsersService
{
    public Task CreateAsync(User user);
    public Task UpdateAsync(User user);
    public Task DeleteAsync(Guid id);
}